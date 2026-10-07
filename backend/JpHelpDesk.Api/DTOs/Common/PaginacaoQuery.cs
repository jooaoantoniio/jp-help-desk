using System.ComponentModel.DataAnnotations;

namespace JpHelpDesk.Api.DTOs.Common;

/// <summary>
/// Parâmetros de paginação recebidos pela query string.
/// </summary>
public class PaginacaoQuery
{
    public const int TamanhoMaximoPagina = 50;

    /// <summary>Número da página (começa em 1).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior ou igual a 1.")]
    public int Pagina { get; set; } = 1;

    /// <summary>Quantidade de itens por página (1 a 50).</summary>
    [Range(1, TamanhoMaximoPagina, ErrorMessage = "O tamanho da página deve estar entre 1 e 50.")]
    public int TamanhoPagina { get; set; } = 10;
}
