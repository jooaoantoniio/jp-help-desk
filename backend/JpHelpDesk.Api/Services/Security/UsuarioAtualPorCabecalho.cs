using JpHelpDesk.Api.Exceptions;
using JpHelpDesk.Api.Repositories;

namespace JpHelpDesk.Api.Services.Security;

/// <summary>
/// TEMPORÁRIO (até a Fase 9 — JWT): identifica o usuário pelo cabeçalho HTTP "X-Usuario-Id".
/// Não é seguro — qualquer cliente pode se passar por outro usuário. Será substituído pela
/// leitura do token JWT, sem mudanças nos serviços que usam <see cref="IUsuarioAtual"/>.
/// </summary>
public class UsuarioAtualPorCabecalho(
    IHttpContextAccessor httpContextAccessor,
    IUsuarioRepository usuarioRepository) : IUsuarioAtual
{
    public const string Cabecalho = "X-Usuario-Id";

    // Scoped: o resultado é reaproveitado durante a mesma requisição.
    private UsuarioLogado? _usuario;

    public async Task<UsuarioLogado> ObterAsync(CancellationToken cancellationToken)
    {
        if (_usuario is not null)
        {
            return _usuario;
        }

        var valor = httpContextAccessor.HttpContext?.Request.Headers[Cabecalho].ToString();

        if (!int.TryParse(valor, out var id))
        {
            throw new NaoAutenticadoException($"Informe o ID do usuário no cabeçalho '{Cabecalho}'.");
        }

        var usuario = await usuarioRepository.ObterPorIdAsync(id, cancellationToken);

        if (usuario is null || !usuario.Ativo)
        {
            throw new NaoAutenticadoException("Usuário não encontrado ou inativo.");
        }

        return _usuario = new UsuarioLogado(usuario.Id, usuario.Nome, usuario.Perfil);
    }
}
