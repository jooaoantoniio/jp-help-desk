using System.ComponentModel.DataAnnotations;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.DTOs.Usuarios;

/// <summary>
/// Dados para cadastrar um usuário.
/// </summary>
public class UsuarioCreateRequest
{
    /// <example>Maria Souza</example>
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [TamanhoTexto(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <example>maria.souza@empresa.com</example>
    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Mínimo de 8 caracteres, com letra maiúscula, minúscula, número e caractere especial.</summary>
    /// <example>Senha@Forte1</example>
    [Required(ErrorMessage = "A senha é obrigatória.")]
    [TamanhoTexto(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
    [RegularExpression(RegrasSenha.Padrao, ErrorMessage = RegrasSenha.Mensagem)]
    public string Senha { get; set; } = string.Empty;

    /// <summary>Perfil de acesso: ADMIN, TECNICO ou USUARIO.</summary>
    /// <example>USUARIO</example>
    [Required(ErrorMessage = "O perfil é obrigatório.")]
    public PerfilUsuario? Perfil { get; set; }
}
