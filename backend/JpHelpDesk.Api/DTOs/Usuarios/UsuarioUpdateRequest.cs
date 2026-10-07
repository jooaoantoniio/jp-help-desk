using System.ComponentModel.DataAnnotations;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.DTOs.Usuarios;

/// <summary>
/// Dados para editar um usuário. A senha não é alterada por aqui.
/// </summary>
public class UsuarioUpdateRequest
{
    /// <example>Maria Souza</example>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <example>maria.souza@empresa.com</example>
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Perfil de acesso: ADMIN, TECNICO ou USUARIO.</summary>
    /// <example>TECNICO</example>
    [Required(ErrorMessage = "O perfil é obrigatório.")]
    public PerfilUsuario? Perfil { get; set; }
}
