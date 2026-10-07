using JpHelpDesk.Api.Models;
using JpHelpDesk.Api.Models.Enums;

namespace JpHelpDesk.Api.DTOs.Usuarios;

/// <summary>
/// Usuário retornado pela API (sem dados sensíveis).
/// </summary>
public record UsuarioResponse(
    int Id,
    string Nome,
    string Email,
    PerfilUsuario Perfil,
    bool Ativo,
    DateTimeOffset CriadoEm,
    DateTimeOffset? AtualizadoEm)
{
    public static UsuarioResponse FromEntity(Usuario usuario) => new(
        usuario.Id,
        usuario.Nome,
        usuario.Email,
        usuario.Perfil,
        usuario.Ativo,
        usuario.CriadoEm,
        usuario.AtualizadoEm);
}
