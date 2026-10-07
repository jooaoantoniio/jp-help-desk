using System.Security.Claims;
using JpHelpDesk.Api.Exceptions;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.Services.Security;

/// <summary>
/// Identifica o usuário atual a partir das claims do token JWT já validado pelo middleware de autenticação.
/// </summary>
public class UsuarioAtualPorToken(IHttpContextAccessor httpContextAccessor) : IUsuarioAtual
{
    public Task<UsuarioLogado> ObterAsync(CancellationToken cancellationToken)
    {
        var principal = httpContextAccessor.HttpContext?.User;

        if (principal?.Identity?.IsAuthenticated != true
            || !int.TryParse(principal.FindFirstValue(JwtTokenService.Claims.Id), out var id)
            || principal.FindFirstValue(JwtTokenService.Claims.Perfil) is not { } perfil)
        {
            throw new NaoAutenticadoException("Usuário não autenticado.");
        }

        var nome = principal.FindFirstValue(JwtTokenService.Claims.Nome) ?? string.Empty;

        return Task.FromResult(new UsuarioLogado(id, nome, EnumExtensions.FromApiString<PerfilUsuario>(perfil)));
    }
}
