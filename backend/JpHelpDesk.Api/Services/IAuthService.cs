using JpHelpDesk.Api.DTOs.Auth;
using JpHelpDesk.Api.DTOs.Usuarios;

namespace JpHelpDesk.Api.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<UsuarioResponse> ObterPerfilAsync(CancellationToken cancellationToken);
    Task AlterarSenhaAsync(AlterarSenhaRequest request, CancellationToken cancellationToken);
}
