using JpHelpDesk.Api.DTOs.Chamados;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Models;

namespace JpHelpDesk.Api.Repositories;

public interface IChamadoRepository
{
    Task<ResultadoPaginado<Chamado>> ListarAsync(ChamadoQuery query, CancellationToken cancellationToken);

    /// <summary>Retorna o chamado com categoria, solicitante e técnico carregados.</summary>
    Task<Chamado?> ObterPorIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExisteAsync(int id, CancellationToken cancellationToken);

    /// <summary>Histórico do chamado em ordem cronológica, com o usuário de cada evento.</summary>
    Task<IReadOnlyList<HistoricoChamado>> ListarHistoricoAsync(int chamadoId, CancellationToken cancellationToken);

    void Adicionar(Chamado chamado);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
