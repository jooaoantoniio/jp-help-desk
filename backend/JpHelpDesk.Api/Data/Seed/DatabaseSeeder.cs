using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using JpHelpDesk.Api.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JpHelpDesk.Api.Data.Seed;

/// <summary>
/// Dados do ambiente de desenvolvimento: usuários de teste e, opcionalmente, chamados de demonstração.
/// É idempotente: o que já existe não é alterado.
/// </summary>
public class DatabaseSeeder(
    AppDbContext context,
    ISenhaHasher senhaHasher,
    IOptions<SeedOptions> options,
    TimeProvider timeProvider,
    ILogger<DatabaseSeeder> logger)
{
    private static readonly (string Nome, string Email, PerfilUsuario Perfil)[] UsuariosTeste =
    [
        ("Administrador", "admin@jphelpdesk.com", PerfilUsuario.Admin),
        ("Técnico de Suporte", "tecnico@jphelpdesk.com", PerfilUsuario.Tecnico),
        ("Usuário Padrão", "usuario@jphelpdesk.com", PerfilUsuario.Usuario)
    ];

    // Solicitantes extras, criados apenas junto com os chamados de demonstração.
    private static readonly (string Nome, string Email, PerfilUsuario Perfil)[] UsuariosDemonstracao =
    [
        ("Maria Oliveira", "maria.oliveira@jphelpdesk.com", PerfilUsuario.Usuario),
        ("Carlos Lima", "carlos.lima@jphelpdesk.com", PerfilUsuario.Usuario)
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var config = options.Value;

        if (string.IsNullOrWhiteSpace(config.SenhaUsuariosTeste))
        {
            logger.LogWarning(
                "Seed ignorado: configure '{Chave}' via User Secrets ou variável de ambiente.",
                $"{SeedOptions.Secao}:{nameof(SeedOptions.SenhaUsuariosTeste)}");
            return;
        }

        var usuarios = config.ChamadosDemonstracao ? [.. UsuariosTeste, .. UsuariosDemonstracao] : UsuariosTeste;
        await CriarUsuariosAsync(usuarios, config.SenhaUsuariosTeste, cancellationToken);

        if (config.ChamadosDemonstracao)
        {
            await new ChamadosDemonstracaoSeeder(context, timeProvider, logger).SeedAsync(cancellationToken);
        }
    }

    private async Task CriarUsuariosAsync(
        IEnumerable<(string Nome, string Email, PerfilUsuario Perfil)> usuarios,
        string senha,
        CancellationToken cancellationToken)
    {
        foreach (var (nome, email, perfil) in usuarios)
        {
            if (await context.Usuarios.AnyAsync(u => u.Email == email, cancellationToken))
            {
                continue;
            }

            context.Usuarios.Add(new Usuario
            {
                Nome = nome,
                Email = email,
                SenhaHash = senhaHasher.GerarHash(senha),
                Perfil = perfil,
                Ativo = true,
                CriadoEm = timeProvider.GetUtcNow()
            });

            logger.LogInformation("Usuário de teste criado: {Email} ({Perfil})", email, perfil);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
