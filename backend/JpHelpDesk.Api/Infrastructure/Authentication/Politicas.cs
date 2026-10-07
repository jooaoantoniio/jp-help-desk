namespace JpHelpDesk.Api.Infrastructure.Authentication;

/// <summary>
/// Nomes das políticas de autorização por perfil, usadas em [Authorize(Policy = ...)].
/// </summary>
public static class Politicas
{
    /// <summary>Somente ADMIN.</summary>
    public const string Admin = "Admin";

    /// <summary>Equipe de suporte: ADMIN ou TECNICO.</summary>
    public const string Equipe = "Equipe";
}
