using JpHelpDesk.Api.DTOs.Chamados;
using JpHelpDesk.Api.DTOs.Common;

namespace JpHelpDesk.Api.Services;

public interface IChamadoService
{
    Task<ResultadoPaginado<ChamadoResponse>> ListarAsync(ChamadoQuery query, CancellationToken cancellationToken);
    Task<ChamadoResponse> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<ChamadoResponse> AbrirAsync(ChamadoRequest request, CancellationToken cancellationToken);
    Task<ChamadoResponse> AtualizarAsync(int id, ChamadoRequest request, CancellationToken cancellationToken);
    Task<ChamadoResponse> AlterarStatusAsync(int id, AlterarStatusRequest request, CancellationToken cancellationToken);
    Task<ChamadoResponse> AtribuirTecnicoAsync(int id, AtribuirTecnicoRequest request, CancellationToken cancellationToken);
    Task CancelarAsync(int id, CancellationToken cancellationToken);
}
