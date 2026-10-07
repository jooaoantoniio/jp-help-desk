using JpHelpDesk.Api.DTOs.Chamados;
using JpHelpDesk.Api.DTOs.Common;
using JpHelpDesk.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JpHelpDesk.Api.Controllers;

/// <summary>
/// Gerenciamento de chamados de suporte.
/// </summary>
[ApiController]
[Route("api/chamados")]
[Produces("application/json")]
public class ChamadosController(IChamadoService chamadoService) : ControllerBase
{
    /// <summary>
    /// Lista chamados com filtros, ordenação e paginação.
    /// </summary>
    /// <response code="200">Página de chamados.</response>
    /// <response code="400">Filtros inválidos.</response>
    [HttpGet]
    [ProducesResponseType<ResultadoPaginado<ChamadoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ResultadoPaginado<ChamadoResponse>>> Listar(
        [FromQuery] ChamadoQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await chamadoService.ListarAsync(query, cancellationToken));
    }

    /// <summary>
    /// Obtém um chamado pelo ID.
    /// </summary>
    /// <param name="id">ID do chamado.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Chamado encontrado.</response>
    /// <response code="404">Chamado não encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ChamadoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChamadoResponse>> ObterPorId(int id, CancellationToken cancellationToken)
    {
        return Ok(await chamadoService.ObterPorIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Abre um novo chamado em nome do usuário atual.
    /// </summary>
    /// <response code="201">Chamado aberto com status ABERTO.</response>
    /// <response code="400">Dados inválidos ou categoria inativa.</response>
    /// <response code="401">Usuário não identificado.</response>
    [HttpPost]
    [ProducesResponseType<ChamadoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ChamadoResponse>> Abrir(
        ChamadoRequest request,
        CancellationToken cancellationToken)
    {
        var chamado = await chamadoService.AbrirAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = chamado.Id }, chamado);
    }

    /// <summary>
    /// Edita título, descrição, prioridade e categoria de um chamado não finalizado.
    /// </summary>
    /// <param name="id">ID do chamado.</param>
    /// <param name="request">Novos dados do chamado.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Chamado atualizado.</response>
    /// <response code="400">Dados inválidos, categoria inativa ou chamado finalizado.</response>
    /// <response code="404">Chamado não encontrado.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ChamadoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChamadoResponse>> Atualizar(
        int id,
        ChamadoRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await chamadoService.AtualizarAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Altera o status do chamado, respeitando o fluxo permitido (veja "proximosStatus").
    /// </summary>
    /// <param name="id">ID do chamado.</param>
    /// <param name="request">Novo status.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Status alterado.</response>
    /// <response code="400">Transição de status não permitida.</response>
    /// <response code="404">Chamado não encontrado.</response>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ChamadoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChamadoResponse>> AlterarStatus(
        int id,
        AlterarStatusRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await chamadoService.AlterarStatusAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Atribui (ou troca) o técnico responsável pelo chamado.
    /// </summary>
    /// <param name="id">ID do chamado.</param>
    /// <param name="request">Técnico responsável.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Técnico atribuído.</response>
    /// <response code="400">Técnico inválido ou chamado em status que não permite atribuição.</response>
    /// <response code="404">Chamado não encontrado.</response>
    [HttpPatch("{id:int}/atribuir")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ChamadoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChamadoResponse>> AtribuirTecnico(
        int id,
        AtribuirTecnicoRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await chamadoService.AtribuirTecnicoAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Cancela um chamado (exclusão lógica — o chamado e seu histórico são preservados).
    /// </summary>
    /// <param name="id">ID do chamado.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="204">Chamado cancelado.</response>
    /// <response code="400">Chamado já resolvido ou fechado.</response>
    /// <response code="404">Chamado não encontrado.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancelar(int id, CancellationToken cancellationToken)
    {
        await chamadoService.CancelarAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Retorna a linha do tempo do chamado (abertura, atribuições, status, edições e comentários).
    /// </summary>
    /// <param name="id">ID do chamado.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Eventos em ordem cronológica.</response>
    /// <response code="404">Chamado não encontrado.</response>
    [HttpGet("{id:int}/historico")]
    [ProducesResponseType<IReadOnlyList<HistoricoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<HistoricoResponse>>> ObterHistorico(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await chamadoService.ListarHistoricoAsync(id, cancellationToken));
    }

    /// <summary>
    /// Adiciona um comentário ao chamado (observação do técnico ou informação do solicitante).
    /// </summary>
    /// <param name="id">ID do chamado.</param>
    /// <param name="request">Texto do comentário.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="201">Comentário registrado no histórico.</response>
    /// <response code="400">Texto inválido ou chamado finalizado.</response>
    /// <response code="401">Usuário não identificado.</response>
    /// <response code="404">Chamado não encontrado.</response>
    [HttpPost("{id:int}/comentarios")]
    [ProducesResponseType<HistoricoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HistoricoResponse>> Comentar(
        int id,
        ComentarioRequest request,
        CancellationToken cancellationToken)
    {
        var comentario = await chamadoService.ComentarAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(ObterHistorico), new { id }, comentario);
    }
}
