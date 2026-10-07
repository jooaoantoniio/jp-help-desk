using JpHelpDesk.Api.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace JpHelpDesk.Api.Data.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Executa a consulta paginada no banco: um COUNT para o total e um SELECT com OFFSET/FETCH.
    /// A consulta deve estar ordenada para a paginação ser estável.
    /// </summary>
    public static async Task<ResultadoPaginado<T>> ToResultadoPaginadoAsync<T>(
        this IQueryable<T> query,
        PaginacaoQuery paginacao,
        CancellationToken cancellationToken = default)
    {
        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .Skip((paginacao.Pagina - 1) * paginacao.TamanhoPagina)
            .Take(paginacao.TamanhoPagina)
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<T>(itens, paginacao.Pagina, paginacao.TamanhoPagina, total);
    }
}
