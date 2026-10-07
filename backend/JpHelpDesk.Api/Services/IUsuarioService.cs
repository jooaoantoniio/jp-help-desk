using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.DTOs.Usuarios;

namespace JpHelpDesk.Api.Services;

public interface IUsuarioService
{
    Task<ResultadoPaginado<UsuarioResponse>> ListarAsync(UsuarioQuery query, CancellationToken cancellationToken);
    Task<UsuarioResponse> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<UsuarioResponse> CriarAsync(UsuarioCreateRequest request, CancellationToken cancellationToken);
    Task<UsuarioResponse> AtualizarAsync(int id, UsuarioUpdateRequest request, CancellationToken cancellationToken);
    Task DesativarAsync(int id, CancellationToken cancellationToken);
    Task AtivarAsync(int id, CancellationToken cancellationToken);
}
