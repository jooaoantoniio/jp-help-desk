using JpHelpDesk.Api.DTOs.Dashboard;
using JpHelpDesk.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JpHelpDesk.Api.Controllers;

/// <summary>
/// Indicadores consolidados dos chamados.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    /// <summary>
    /// Retorna totais, contagens por status/prioridade/categoria e os chamados mais recentes.
    /// Para o perfil USUARIO, considera apenas os chamados abertos por ele.
    /// </summary>
    /// <response code="200">Indicadores do dashboard.</response>
    /// <response code="401">Token ausente, inválido ou expirado.</response>
    [HttpGet]
    [ProducesResponseType<DashboardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DashboardResponse>> Obter(CancellationToken cancellationToken)
    {
        return Ok(await dashboardService.ObterAsync(cancellationToken));
    }
}
