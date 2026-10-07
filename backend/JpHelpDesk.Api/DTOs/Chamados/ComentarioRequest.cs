using System.ComponentModel.DataAnnotations;
using JpHelpDesk.Api.DTOs.Common;

namespace JpHelpDesk.Api.DTOs.Chamados;

/// <summary>
/// Comentário adicionado ao chamado (observação do técnico ou informação do usuário).
/// </summary>
public class ComentarioRequest
{
    /// <example>O erro também acontece na impressora do 2º andar.</example>
    [Required(ErrorMessage = "O texto do comentário é obrigatório.")]
    [TamanhoTexto(1000, MinimumLength = 2, ErrorMessage = "O comentário deve ter entre 2 e 1000 caracteres.")]
    public string Texto { get; set; } = string.Empty;
}
