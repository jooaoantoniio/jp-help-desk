using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.Services.Security;

/// <summary>Usuário que está realizando a requisição atual.</summary>
public record UsuarioLogado(int Id, string Nome, PerfilUsuario Perfil);

/// <summary>
/// Identifica o usuário da requisição atual. Os serviços dependem só desta interface,
/// sem saber se a identificação vem de um cabeçalho, de um token JWT etc.
/// </summary>
public interface IUsuarioAtual
{
    /// <exception cref="Exceptions.NaoAutenticadoException">Quando não há usuário válido.</exception>
    Task<UsuarioLogado> ObterAsync(CancellationToken cancellationToken);
}
