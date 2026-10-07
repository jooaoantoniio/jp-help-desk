using System.Linq.Expressions;
using JpHelpDesk.Api.Data;
using JpHelpDesk.Api.Data.Extensions;
using JpHelpDesk.Api.DTOs.Chamados;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace JpHelpDesk.Api.Repositories;

public class ChamadoRepository(AppDbContext context) : IChamadoRepository
{
    // Enums são gravados como texto; para ordenar por gravidade (e não alfabeticamente)
    // convertemos em número. O EF traduz este ternário em um CASE WHEN no SQL.
    private static readonly Expression<Func<Chamado, int>> PesoPrioridade = c =>
        c.Prioridade == PrioridadeChamado.Critica ? 4 :
        c.Prioridade == PrioridadeChamado.Alta ? 3 :
        c.Prioridade == PrioridadeChamado.Media ? 2 : 1;

    // Ordem do ciclo de vida do chamado.
    private static readonly Expression<Func<Chamado, int>> OrdemStatus = c =>
        c.Status == StatusChamado.Aberto ? 1 :
        c.Status == StatusChamado.EmAtendimento ? 2 :
        c.Status == StatusChamado.AguardandoUsuario ? 3 :
        c.Status == StatusChamado.Resolvido ? 4 :
        c.Status == StatusChamado.Fechado ? 5 : 6;

    public async Task<ResultadoPaginado<Chamado>> ListarAsync(ChamadoQuery query, CancellationToken cancellationToken)
    {
        var consulta = ComReferencias(context.Chamados.AsNoTracking());

        if (query.Id.HasValue)
            consulta = consulta.Where(c => c.Id == query.Id.Value);

        if (!string.IsNullOrWhiteSpace(query.Titulo))
            consulta = consulta.Where(c => c.Titulo.Contains(query.Titulo.Trim()));

        if (query.Status.HasValue)
            consulta = consulta.Where(c => c.Status == query.Status.Value);

        if (query.Prioridade.HasValue)
            consulta = consulta.Where(c => c.Prioridade == query.Prioridade.Value);

        if (query.CategoriaId.HasValue)
            consulta = consulta.Where(c => c.CategoriaId == query.CategoriaId.Value);

        if (query.SolicitanteId.HasValue)
            consulta = consulta.Where(c => c.SolicitanteId == query.SolicitanteId.Value);

        if (query.TecnicoId.HasValue)
            consulta = consulta.Where(c => c.TecnicoId == query.TecnicoId.Value);

        if (query.AbertoDe.HasValue)
            consulta = consulta.Where(c => c.DataAbertura >= query.AbertoDe.Value);

        if (query.AbertoAte.HasValue)
            consulta = consulta.Where(c => c.DataAbertura <= query.AbertoAte.Value);

        return await Ordenar(consulta, query.OrdenarPor, query.Decrescente)
            .ToResultadoPaginadoAsync(query, cancellationToken);
    }

    public Task<Chamado?> ObterPorIdAsync(int id, CancellationToken cancellationToken) =>
        ComReferencias(context.Chamados).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<HistoricoChamado>> ListarHistoricoAsync(int chamadoId, CancellationToken cancellationToken) =>
        await context.HistoricosChamado
            .AsNoTracking()
            .Include(h => h.Usuario)
            .Where(h => h.ChamadoId == chamadoId)
            .OrderBy(h => h.DataRegistro)
            .ThenBy(h => h.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<StatusChamado, int>> ContarPorStatusAsync(int? solicitanteId, CancellationToken cancellationToken) =>
        await DoSolicitante(solicitanteId)
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Quantidade, cancellationToken);

    public async Task<IReadOnlyDictionary<PrioridadeChamado, int>> ContarPorPrioridadeAsync(int? solicitanteId, CancellationToken cancellationToken) =>
        await DoSolicitante(solicitanteId)
            .GroupBy(c => c.Prioridade)
            .Select(g => new { Prioridade = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.Prioridade, x => x.Quantidade, cancellationToken);

    public async Task<IReadOnlyList<(int CategoriaId, string Categoria, int Quantidade)>> ContarPorCategoriaAsync(
        int? solicitanteId,
        CancellationToken cancellationToken)
    {
        var contagens = await DoSolicitante(solicitanteId)
            .GroupBy(c => new { c.CategoriaId, c.Categoria.Nome })
            .Select(g => new { g.Key.CategoriaId, g.Key.Nome, Quantidade = g.Count() })
            .OrderByDescending(x => x.Quantidade)
            .ThenBy(x => x.Nome)
            .ToListAsync(cancellationToken);

        return contagens.Select(x => (x.CategoriaId, x.Nome, x.Quantidade)).ToList();
    }

    public Task<int> ContarCriticosEmAbertoAsync(int? solicitanteId, CancellationToken cancellationToken) =>
        DoSolicitante(solicitanteId)
            .CountAsync(c => c.Prioridade == PrioridadeChamado.Critica
                && FluxoStatusChamado.StatusEmAberto.Contains(c.Status), cancellationToken);

    public void Adicionar(Chamado chamado) => context.Chamados.Add(chamado);

    /// <summary>Chamados de um solicitante, ou todos quando solicitanteId é null.</summary>
    private IQueryable<Chamado> DoSolicitante(int? solicitanteId) =>
        solicitanteId is null
            ? context.Chamados.AsNoTracking()
            : context.Chamados.AsNoTracking().Where(c => c.SolicitanteId == solicitanteId);

    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);

    /// <summary>Carrega as entidades relacionadas exibidas na resposta (JOINs).</summary>
    private static IQueryable<Chamado> ComReferencias(IQueryable<Chamado> consulta) =>
        consulta
            .Include(c => c.Categoria)
            .Include(c => c.Solicitante)
            .Include(c => c.Tecnico);

    private static IQueryable<Chamado> Ordenar(IQueryable<Chamado> consulta, OrdenacaoChamado campo, bool decrescente)
    {
        var ordenada = campo switch
        {
            OrdenacaoChamado.Prioridade => OrdenarPor(consulta, PesoPrioridade, decrescente),
            OrdenacaoChamado.Status => OrdenarPor(consulta, OrdemStatus, decrescente),
            OrdenacaoChamado.Titulo => OrdenarPor(consulta, c => c.Titulo, decrescente),
            OrdenacaoChamado.Id => OrdenarPor(consulta, c => c.Id, decrescente),
            _ => OrdenarPor(consulta, c => c.DataAbertura, decrescente)
        };

        // Desempate pelo ID: garante paginação estável quando há valores iguais.
        return decrescente ? ordenada.ThenByDescending(c => c.Id) : ordenada.ThenBy(c => c.Id);
    }

    private static IOrderedQueryable<Chamado> OrdenarPor<TChave>(
        IQueryable<Chamado> consulta,
        Expression<Func<Chamado, TChave>> chave,
        bool decrescente) =>
        decrescente ? consulta.OrderByDescending(chave) : consulta.OrderBy(chave);
}
