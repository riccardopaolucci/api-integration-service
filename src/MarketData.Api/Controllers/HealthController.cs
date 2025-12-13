using MarketData.Api.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketData.Api.Controllers;

/// <summary>
/// Health check endpoints for the API and its dependencies.
/// </summary>
[ApiController]
[Route("healthz")]
public class HealthController : ControllerBase
{
    private readonly MarketDataDbContext _dbContext;

    public HealthController(MarketDataDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Returns basic health information, including DB reachability.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            await _dbContext.Database.CanConnectAsync();
            return Ok(new { status = "ok", db = "reachable" });
        }
        catch
        {
            return StatusCode(503, new { status = "degraded", db = "unreachable" });
        }
    }
}
