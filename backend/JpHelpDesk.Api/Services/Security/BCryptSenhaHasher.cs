namespace JpHelpDesk.Api.Services.Security;

/// <summary>
/// Hash de senha com BCrypt: inclui salt aleatório e custo configurável (lento de propósito,
/// para dificultar ataques de força bruta).
/// </summary>
public class BCryptSenhaHasher : ISenhaHasher
{
    // Custo 12 = 2^12 iterações. Cada +1 dobra o tempo de cálculo.
    private const int FatorCusto = 12;

    public string GerarHash(string senha) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(senha, FatorCusto);

    public bool Verificar(string senha, string hash) =>
        BCrypt.Net.BCrypt.EnhancedVerify(senha, hash);
}
