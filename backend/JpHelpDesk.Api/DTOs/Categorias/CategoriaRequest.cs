using System.ComponentModel.DataAnnotations;

namespace JpHelpDesk.Api.DTOs.Categorias;

/// <summary>
/// Dados para criar ou atualizar uma categoria.
/// </summary>
public class CategoriaRequest
{
    /// <summary>Nome da categoria (único).</summary>
    /// <example>Hardware</example>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 50 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Descrição opcional da categoria.</summary>
    /// <example>Computadores, notebooks e periféricos</example>
    [StringLength(250, ErrorMessage = "A descrição deve ter no máximo 250 caracteres.")]
    public string? Descricao { get; set; }
}
