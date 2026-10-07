using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using JpHelpDesk.Api.Data;
using JpHelpDesk.Api.Data.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: AssemblyFixture(typeof(JpHelpDesk.Api.Tests.ApiFactory))]

namespace JpHelpDesk.Api.Tests;

/// <summary>
/// Sobe a API inteira em memória (TestServer) apontando para um banco de testes descartável,
/// recriado a cada execução. Compartilhada por todos os testes de integração (assembly fixture).
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>Senha dos usuários de teste do seed — só existe neste banco descartável.</summary>
    public const string SenhaTeste = "Teste@Integracao1";

    public const string Admin = "admin@jphelpdesk.com";
    public const string Tecnico = "tecnico@jphelpdesk.com";
    public const string Usuario = "usuario@jphelpdesk.com";

    /// <summary>
    /// Banco de testes. Padrão: SQL Server Express local; em outro ambiente (ex.: CI ou Docker)
    /// defina a variável JPHELPDESK_TESTES_CONNECTION.
    /// </summary>
    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("JPHELPDESK_TESTES_CONNECTION")
        ?? @"Server=.\SQLEXPRESS;Database=JpHelpDesk_Testes;Trusted_Connection=True;TrustServerCertificate=True";

    private readonly ConcurrentDictionary<string, string> _tokens = new();

    /// <summary>Limite de logins por minuto. Alto por padrão: todas as requisições de teste vêm do mesmo "IP".</summary>
    protected virtual int TentativasLoginPorMinuto => 10_000;

    /// <summary>Se false, não toca no banco (para fábricas auxiliares que não precisam de dados).</summary>
    protected virtual bool PrepararBanco => true;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing": sem User Secrets e sem appsettings.Development.json — nada de segredo real nos testes.
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", ConnectionString);
        builder.UseSetting("Jwt:ChaveSecreta", Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)));
        builder.UseSetting("Seed:SenhaUsuariosTeste", SenhaTeste);
        builder.UseSetting("Seed:ChamadosDemonstracao", "false");
        builder.UseSetting("Seguranca:TentativasLoginPorMinuto", TentativasLoginPorMinuto.ToString());
        builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
    }

    public async ValueTask InitializeAsync()
    {
        if (!PrepararBanco)
        {
            return;
        }

        using var escopo = Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
        await banco.Database.EnsureDeletedAsync();
        await banco.Database.MigrateAsync(); // mesmas migrations da aplicação (inclui as categorias)
        await escopo.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        if (PrepararBanco)
        {
            using var escopo = Services.CreateScope();
            await escopo.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureDeletedAsync();
        }

        await base.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>Cliente HTTP autenticado como o usuário informado (o token é reaproveitado entre os testes).</summary>
    public async Task<HttpClient> ClienteComoAsync(string email, string senha = SenhaTeste)
    {
        var token = _tokens.TryGetValue(email, out var salvo) ? salvo : _tokens[email] = await LoginAsync(email, senha);
        return ClienteComToken(token);
    }

    public HttpClient ClienteComToken(string token)
    {
        var cliente = CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    public async Task<string> LoginAsync(string email, string senha = SenhaTeste)
    {
        var resposta = await CreateClient().PostAsJsonAsync("/api/auth/login", new { email, senha });
        resposta.EnsureSuccessStatusCode();
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        return corpo.GetProperty("token").GetString()!;
    }

    /// <summary>Cria um usuário novo (via API, como ADMIN) com e-mail único; devolve id e e-mail.</summary>
    public async Task<(int Id, string Email)> CriarUsuarioAsync(string perfil = "USUARIO")
    {
        var email = $"teste.{Guid.NewGuid():N}@jphelpdesk.com";
        var admin = await ClienteComoAsync(Admin);
        var resposta = await admin.PostAsJsonAsync("/api/usuarios", new { nome = "Usuário de Teste", email, senha = SenhaTeste, perfil });
        resposta.EnsureSuccessStatusCode();
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        return (corpo.GetProperty("id").GetInt32(), email);
    }
}
