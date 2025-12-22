using MarketData.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MarketData.Api.Controllers;

/// <summary>
/// Health check endpoints for the API and its dependencies.
/// </summary>
[ApiController]
[Route("healthz")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;

    public HealthController(IHealthService healthService)
    {
        _healthService = healthService;
    }

    /// <summary>
    /// Returns health information for database and external market provider.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var health = await _healthService.GetHealthAsync();

        if (health.Status == "ok")
        {
            return Ok(health);
        }

        return StatusCode(StatusCodes.Status503ServiceUnavailable, health);
    }
}

