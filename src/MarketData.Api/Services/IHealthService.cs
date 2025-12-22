namespace MarketData.Api.Services;

/// <summary>
/// Performs health checks on core dependencies (database, external APIs).
/// </summary>
public interface IHealthService
{
    /// <summary>
    /// Runs health checks and returns a consolidated status payload.
    /// </summary>
    Task<HealthStatusDto> GetHealthAsync();
}
