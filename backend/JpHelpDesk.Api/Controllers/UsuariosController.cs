using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.DTOs.Usuarios;
using JpHelpDesk.Api.Infrastructure.Authentication;
using JpHelpDesk.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JpHelpDesk.Api.Controllers;

/// <summary>
/// Gerenciamento de usuários do sistema (somente ADMIN).
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Authorize(Policy = Politicas.Admin)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    /// <summary>
    /// Lista os usuários com paginação e filtros.
    /// </summary>
    /// <response code="200">Página de usuários.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<UsuarioResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginado<UsuarioResponse>>> Listar(
        [FromQuery] UsuarioQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await usuarioService.ListarAsync(query, cancellationToken));
    }

    /// <summary>
    /// Obtém um usuário pelo ID.
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Usuário encontrado.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> ObterPorId(int id, CancellationToken cancellationToken)
    {
        return Ok(await usuarioService.ObterPorIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Cadastra um novo usuário.
    /// </summary>
    /// <response code="201">Usuário criado.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="409">E-mail já cadastrado.</response>
    [HttpPost]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> Criar(
        UsuarioCreateRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
    }

    /// <summary>
    /// Atualiza nome, e-mail e perfil de um usuário.
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <param name="request">Novos dados do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Usuário atualizado.</response>
    /// <response code="400">Dados inválidos ou remoção do último administrador.</response>
    /// <response code="404">Usuário não encontrado.</response>
    /// <response code="409">E-mail já cadastrado por outro usuário.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType<UsuarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> Atualizar(
        int id,
        UsuarioUpdateRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await usuarioService.AtualizarAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Desativa um usuário (exclusão lógica — o histórico de chamados é preservado).
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="204">Usuário desativado.</response>
    /// <response code="400">Não é possível desativar o último administrador ativo.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(int id, CancellationToken cancellationToken)
    {
        await usuarioService.DesativarAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Reativa um usuário desativado.
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="204">Usuário ativado.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpPatch("{id:int}/ativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ativar(int id, CancellationToken cancellationToken)
    {
        await usuarioService.AtivarAsync(id, cancellationToken);
        return NoContent();
    }
}
