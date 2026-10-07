using JpHelpDesk.Api.DTOs.Categorias;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Infrastructure.Authentication;
using JpHelpDesk.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JpHelpDesk.Api.Controllers;

/// <summary>
/// Categorias de chamados. Leitura para qualquer usuário autenticado; alterações somente ADMIN.
/// </summary>
[ApiController]
[Route("api/categorias")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
public class CategoriasController(ICategoriaService categoriaService) : ControllerBase
{
    /// <summary>
    /// Lista as categorias com paginação e filtros. Perfis que não são ADMIN veem apenas as ativas.
    /// </summary>
    /// <response code="200">Página de categorias.</response>
    /// <response code="400">Parâmetros de paginação inválidos.</response>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<CategoriaResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginado<CategoriaResponse>>> Listar(
        [FromQuery] CategoriaQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await categoriaService.ListarAsync(query, cancellationToken));
    }

    /// <summary>
    /// Obtém uma categoria pelo ID.
    /// </summary>
    /// <param name="id">ID da categoria.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Categoria encontrada.</response>
    /// <response code="404">Categoria não encontrada.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<CategoriaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaResponse>> ObterPorId(int id, CancellationToken cancellationToken)
    {
        return Ok(await categoriaService.ObterPorIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Cria uma nova categoria.
    /// </summary>
    /// <response code="201">Categoria criada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="409">Já existe uma categoria com o mesmo nome.</response>
    [HttpPost]
    [Authorize(Policy = Politicas.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<CategoriaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoriaResponse>> Criar(
        CategoriaRequest request,
        CancellationToken cancellationToken)
    {
        var categoria = await categoriaService.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
    }

    /// <summary>
    /// Atualiza nome e descrição de uma categoria.
    /// </summary>
    /// <param name="id">ID da categoria.</param>
    /// <param name="request">Novos dados da categoria.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Categoria atualizada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Categoria não encontrada.</response>
    /// <response code="409">Já existe outra categoria com o mesmo nome.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Politicas.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<CategoriaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoriaResponse>> Atualizar(
        int id,
        CategoriaRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await categoriaService.AtualizarAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Desativa uma categoria (exclusão lógica — os chamados vinculados são preservados).
    /// </summary>
    /// <param name="id">ID da categoria.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="204">Categoria desativada.</response>
    /// <response code="404">Categoria não encontrada.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Politicas.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Desativar(int id, CancellationToken cancellationToken)
    {
        await categoriaService.DesativarAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Reativa uma categoria desativada.
    /// </summary>
    /// <param name="id">ID da categoria.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="204">Categoria ativada.</response>
    /// <response code="404">Categoria não encontrada.</response>
    [HttpPatch("{id:int}/ativar")]
    [Authorize(Policy = Politicas.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Ativar(int id, CancellationToken cancellationToken)
    {
        await categoriaService.AtivarAsync(id, cancellationToken);
        return NoContent();
    }
}
