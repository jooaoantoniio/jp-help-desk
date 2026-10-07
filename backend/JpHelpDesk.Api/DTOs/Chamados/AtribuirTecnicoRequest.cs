using System.ComponentModel.DataAnnotations;

namespace JpHelpDesk.Api.DTOs.Chamados;

/// <summary>
/// Técnico que ficará responsável pelo chamado.
/// </summary>
public class AtribuirTecnicoRequest
{
    /// <summary>ID de um usuário ativo com perfil TECNICO ou ADMIN.</summary>
    /// <example>2</example>
    [Required(ErrorMessage = "O técnico é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "Técnico inválido.")]
    public int? TecnicoId { get; set; }
}
