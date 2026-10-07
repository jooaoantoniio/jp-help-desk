using System.ComponentModel.DataAnnotations;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.DTOs.Chamados;

/// <summary>
/// Novo status do chamado.
/// </summary>
public class AlterarStatusRequest
{
    /// <summary>ABERTO, EM_ATENDIMENTO, AGUARDANDO_USUARIO, RESOLVIDO, FECHADO ou CANCELADO.</summary>
    /// <example>RESOLVIDO</example>
    [Required(ErrorMessage = "O status é obrigatório.")]
    public StatusChamado? Status { get; set; }

    /// <summary>Observação opcional registrada no histórico (ex.: solução aplicada).</summary>
    /// <example>Toner substituído e impressora testada.</example>
    [StringLength(500, ErrorMessage = "A observação deve ter no máximo 500 caracteres.")]
    public string? Observacao { get; set; }
}
