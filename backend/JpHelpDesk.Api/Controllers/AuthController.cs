using JpHelpDesk.Api.DTOs.Auth;
using JpHelpDesk.Api.DTOs.Usuarios;
using JpHelpDesk.Api.Infrastructure.Authentication;
using JpHelpDesk.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace JpHelpDesk.Api.Controllers;

/// <summary>
/// Autenticação e dados do usuário logado.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Autentica com e-mail e senha e retorna um token JWT.
    /// </summary>
    /// <response code="200">Login realizado.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="401">E-mail ou senha inválidos.</response>
    /// <response code="429">Muitas tentativas. Aguarde e tente novamente.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthenticationExtensions.PoliticaLimiteLogin)]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return Ok(await authService.LoginAsync(request, cancellationToken));
    }

    /// <summary>
    /// Retorna os dados do usuário autenticado.
    /// </summary>
    /// <response code="200">Perfil do usuário.</response>
    /// <response code="401">Token ausente, inválido ou expirado.</response>
    [HttpGet("me")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioResponse>> Me(CancellationToken cancellationToken)
    {
        return Ok(await authService.ObterPerfilAsync(cancellationToken));
    }

    /// <summary>
    /// Altera a senha do usuário autenticado.
    /// </summary>
    /// <response code="204">Senha alterada.</response>
    /// <response code="400">Senha atual incorreta ou nova senha inválida.</response>
    /// <response code="401">Token ausente, inválido ou expirado.</response>
    [HttpPut("senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaRequest request, CancellationToken cancellationToken)
    {
        await authService.AlterarSenhaAsync(request, cancellationToken);
        return NoContent();
    }
}
