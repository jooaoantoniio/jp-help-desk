using JpHelpDesk.Api.DTOs.Common;

namespace JpHelpDesk.Api.DTOs.Categorias;

/// <summary>
/// Filtros da listagem de categorias.
/// </summary>
public class CategoriaQuery : PaginacaoQuery
{
    /// <summary>Texto contido no nome da categoria.</summary>
    public string? Busca { get; set; }

    /// <summary>Filtra por situação (true = ativas, false = inativas). Vazio retorna todas.</summary>
    public bool? Ativo { get; set; }
}
