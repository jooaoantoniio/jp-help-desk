using System.ComponentModel.DataAnnotations;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.DTOs.Usuarios;

namespace JpHelpDesk.Api.DTOs.Auth;

/// <summary>
/// Troca da senha do próprio usuário.
/// </summary>
public class AlterarSenhaRequest
{
    [Required(ErrorMessage = "A senha atual é obrigatória.")]
    public string SenhaAtual { get; set; } = string.Empty;

    /// <summary>Mínimo de 8 caracteres, com letra maiúscula, minúscula, número e caractere especial.</summary>
    [Required(ErrorMessage = "A nova senha é obrigatória.")]
    [TamanhoTexto(100, MinimumLength = 8, ErrorMessage = "A nova senha deve ter entre 8 e 100 caracteres.")]
    [RegularExpression(RegrasSenha.Padrao, ErrorMessage = RegrasSenha.Mensagem)]
    public string NovaSenha { get; set; } = string.Empty;
}
