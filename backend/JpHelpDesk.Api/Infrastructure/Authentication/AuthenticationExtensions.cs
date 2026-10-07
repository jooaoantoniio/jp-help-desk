using System.Security.Claims;
using System.Threading.RateLimiting;
using JpHelpDesk.Api.Models.Enums;
using JpHelpDesk.Api.Repositories;
using JpHelpDesk.Api.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JpHelpDesk.Api.Infrastructure.Authentication;

public static class AuthenticationExtensions
{
    public const string PoliticaLimiteLogin = "login";

    /// <summary>
    /// Configura autenticação JWT Bearer, autorização (protegido por padrão) e limite de tentativas de login.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Valida a configuração na inicialização: sem chave válida, a API nem sobe.
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.Secao))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUsuarioAtual, UsuarioAtualPorToken>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        // Configurado a partir de IOptions<JwtOptions> (resolvido só quando a aplicação já está montada),
        // o que permite aos testes substituir a configuração.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;

                // Mantém os nomes originais das claims ("sub", "role"...), sem o mapeamento legado da Microsoft.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Emissor,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audiencia,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = JwtTokenService.CriarChave(jwt.ChaveSecreta),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtTokenService.Claims.Nome,
                    RoleClaimType = JwtTokenService.Claims.Perfil
                };

                options.Events = new JwtBearerEvents
                {
                    // Token válido não basta: o usuário precisa continuar ativo e com o mesmo perfil.
                    // Assim, desativar alguém ou mudar seu perfil revoga os tokens já emitidos.
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal!;
                        var repositorio = context.HttpContext.RequestServices.GetRequiredService<IUsuarioRepository>();

                        if (!int.TryParse(principal.FindFirstValue(JwtTokenService.Claims.Id), out var id))
                        {
                            context.Fail("Token sem identificação do usuário.");
                            return;
                        }

                        var perfilAtual = await repositorio.ObterPerfilSeAtivoAsync(id, context.HttpContext.RequestAborted);

                        if (perfilAtual is null || perfilAtual.Value.ToApiString() != principal.FindFirstValue(JwtTokenService.Claims.Perfil))
                        {
                            context.Fail("Usuário inativo ou com perfil alterado. Faça login novamente.");
                        }
                    }
                };
            });

        // Protegido por padrão: todo endpoint exige usuário autenticado, exceto os marcados com [AllowAnonymous].
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        // Limita tentativas de login por IP (proteção contra força bruta).
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(PoliticaLimiteLogin, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}
