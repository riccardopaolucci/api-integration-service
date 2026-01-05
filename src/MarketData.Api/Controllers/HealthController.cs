using MarketData.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MarketData.Api.Controllers;

/// <summary>
/// Health check endpoints for the API.
/// - /healthz = liveness (app is running) -> always 200
/// - /readyz  = readiness (deps ready)    -> 200 or 503
/// </summary>
[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;

    public HealthController(IHealthService healthService)
    {
        _healthService = healthService;
    }

    /// <summary>
    /// Liveness probe: confirms the API process is running.
    /// Always returns 200 OK (even if dependencies are degraded).
    /// </summary>
    [HttpGet("healthz")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Healthz()
    {
        var health = await _healthService.GetHealthAsync();
        return Ok(health);
    }

    /// <summary>
    /// Readiness probe: confirms dependencies (e.g., DB / external provider) are ready.
    /// Returns 200 when healthy, otherwise 503.
    /// </summary>
    [HttpGet("readyz")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Readyz()
    {
        var health = await _healthService.GetHealthAsync();

        return health.Status == "ok"
            ? Ok(health)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, health);
    }
}

