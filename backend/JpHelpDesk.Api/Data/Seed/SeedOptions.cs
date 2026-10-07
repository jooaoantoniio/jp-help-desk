namespace JpHelpDesk.Api.Data.Seed;

/// <summary>
/// Configuração do seed de desenvolvimento (seção "Seed").
/// </summary>
public class SeedOptions
{
    public const string Secao = "Seed";

    /// <summary>
    /// Senha dos usuários de teste. Deve vir de User Secrets ou variável de ambiente — nunca do Git.
    /// </summary>
    public string? SenhaUsuariosTeste { get; set; }
}
