using System.ComponentModel.DataAnnotations;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.DTOs.Chamados;

/// <summary>
/// Novo status do chamado.
/// </summary>
public class AlterarStatusRequest
{
    /// <summary>ABERTO, EM_ATENDIMENTO, AGUARDANDO_USUARIO, RESOLVIDO, FECHADO ou CANCELADO.</summary>
    /// <example>EM_ATENDIMENTO</example>
    [Required(ErrorMessage = "O status é obrigatório.")]
    public StatusChamado? Status { get; set; }
}
