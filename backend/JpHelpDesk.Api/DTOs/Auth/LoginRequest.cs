using System.ComponentModel.DataAnnotations;

namespace JpHelpDesk.Api.DTOs.Auth;

/// <summary>
/// Credenciais de acesso.
/// </summary>
public class LoginRequest
{
    /// <example>admin@jphelpdesk.com</example>
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    public string Senha { get; set; } = string.Empty;
}
