using JpHelpDesk.Api.DTOs.Usuarios;

namespace JpHelpDesk.Api.DTOs.Auth;

/// <summary>
/// Token de acesso e dados do usuário autenticado.
/// Envie o token nas próximas requisições no cabeçalho "Authorization: Bearer {token}".
/// </summary>
public record LoginResponse(
    string Token,
    string TipoToken,
    DateTimeOffset ExpiraEm,
    UsuarioResponse Usuario);
