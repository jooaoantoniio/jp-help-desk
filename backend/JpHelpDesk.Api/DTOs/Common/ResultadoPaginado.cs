namespace JpHelpDesk.Api.DTOs.Common;

/// <summary>
/// Página de resultados de uma listagem.
/// </summary>
public record ResultadoPaginado<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens)
{
    public int TotalPaginas => TotalItens == 0 ? 0 : (int)Math.Ceiling(TotalItens / (double)TamanhoPagina);

    /// <summary>Converte os itens mantendo os dados de paginação (ex.: entidade -> DTO).</summary>
    public ResultadoPaginado<TDestino> Map<TDestino>(Func<T, TDestino> conversor) =>
        new(Itens.Select(conversor).ToList(), Pagina, TamanhoPagina, TotalItens);
}
