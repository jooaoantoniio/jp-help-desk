using JpHelpDesk.Api.DTOs.Categorias;
using JpHelpDesk.Api.DTOs.Common;

namespace JpHelpDesk.Api.Services;

public interface ICategoriaService
{
    Task<ResultadoPaginado<CategoriaResponse>> ListarAsync(CategoriaQuery query, CancellationToken cancellationToken);
    Task<CategoriaResponse> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<CategoriaResponse> CriarAsync(CategoriaRequest request, CancellationToken cancellationToken);
    Task<CategoriaResponse> AtualizarAsync(int id, CategoriaRequest request, CancellationToken cancellationToken);
    Task DesativarAsync(int id, CancellationToken cancellationToken);
    Task AtivarAsync(int id, CancellationToken cancellationToken);
}
