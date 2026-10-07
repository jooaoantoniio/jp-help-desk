using JpHelpDesk.Api.DTOs.Chamados;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.Repositories;

public interface IChamadoRepository
{
    Task<ResultadoPaginado<Chamado>> ListarAsync(ChamadoQuery query, CancellationToken cancellationToken);

    /// <summary>Retorna o chamado com categoria, solicitante e técnico carregados.</summary>
    Task<Chamado?> ObterPorIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>Histórico do chamado em ordem cronológica, com o usuário de cada evento.</summary>
    Task<IReadOnlyList<HistoricoChamado>> ListarHistoricoAsync(int chamadoId, CancellationToken cancellationToken);

    // ---------- Estatísticas (solicitanteId = null considera todos os chamados) ----------

    Task<IReadOnlyDictionary<StatusChamado, int>> ContarPorStatusAsync(int? solicitanteId, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<PrioridadeChamado, int>> ContarPorPrioridadeAsync(int? solicitanteId, CancellationToken cancellationToken);
    Task<IReadOnlyList<(int CategoriaId, string Categoria, int Quantidade)>> ContarPorCategoriaAsync(int? solicitanteId, CancellationToken cancellationToken);
    Task<int> ContarCriticosEmAbertoAsync(int? solicitanteId, CancellationToken cancellationToken);

    void Adicionar(Chamado chamado);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
