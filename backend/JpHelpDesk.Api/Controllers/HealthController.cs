using JpHelpDesk.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace JpHelpDesk.Api.Controllers;

/// <summary>
/// Verificação de disponibilidade da API.
/// </summary>
[ApiController]
[Route("api/health")]
[Produces("application/json")]
public class HealthController(IWebHostEnvironment environment, TimeProvider timeProvider) : ControllerBase
{
    /// <summary>
    /// Retorna o status atual da API.
    /// </summary>
    /// <response code="200">A API está em execução.</response>
    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Get()
    {
        var version = typeof(HealthController).Assembly.GetName().Version?.ToString() ?? "unknown";

        return Ok(new HealthResponse(
            Status: "Healthy",
            Version: version,
            Environment: environment.EnvironmentName,
            Timestamp: timeProvider.GetUtcNow()));
    }
}
