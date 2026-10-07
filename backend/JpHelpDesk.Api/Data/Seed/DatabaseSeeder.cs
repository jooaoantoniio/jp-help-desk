using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using JpHelpDesk.Api.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JpHelpDesk.Api.Data.Seed;

/// <summary>
/// Cria os usuários de teste no ambiente de desenvolvimento. É idempotente:
/// usuários que já existem (pelo e-mail) não são alterados.
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

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var senha = options.Value.SenhaUsuariosTeste;

        if (string.IsNullOrWhiteSpace(senha))
        {
            logger.LogWarning(
                "Seed de usuários ignorado: configure '{Chave}' via User Secrets ou variável de ambiente.",
                $"{SeedOptions.Secao}:{nameof(SeedOptions.SenhaUsuariosTeste)}");
            return;
        }

        foreach (var (nome, email, perfil) in UsuariosTeste)
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
