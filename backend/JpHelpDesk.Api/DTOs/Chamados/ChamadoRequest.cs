using System.ComponentModel.DataAnnotations;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.DTOs.Chamados;

/// <summary>
/// Dados para abrir ou editar um chamado.
/// </summary>
public class ChamadoRequest
{
    /// <example>Impressora do financeiro não imprime</example>
    [Required(ErrorMessage = "O título é obrigatório.")]
    [TamanhoTexto(150, MinimumLength = 5, ErrorMessage = "O título deve ter entre 5 e 150 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    /// <example>Ao enviar qualquer documento, a impressora exibe "erro de papel" mesmo com a bandeja cheia.</example>
    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [TamanhoTexto(4000, MinimumLength = 10, ErrorMessage = "A descrição deve ter entre 10 e 4000 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Prioridade: BAIXA, MEDIA, ALTA ou CRITICA.</summary>
    /// <example>MEDIA</example>
    [Required(ErrorMessage = "A prioridade é obrigatória.")]
    public PrioridadeChamado? Prioridade { get; set; }

    /// <summary>ID de uma categoria ativa.</summary>
    /// <example>4</example>
    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [Range(1, int.MaxValue, ErrorMessage = "Categoria inválida.")]
    public int? CategoriaId { get; set; }
}
