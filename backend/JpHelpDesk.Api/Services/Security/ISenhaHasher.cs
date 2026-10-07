namespace JpHelpDesk.Api.Services.Security;

/// <summary>
/// Gera e verifica hashes de senha. A senha em texto puro nunca é armazenada.
/// </summary>
public interface ISenhaHasher
{
    string GerarHash(string senha);
    bool Verificar(string senha, string hash);
}
