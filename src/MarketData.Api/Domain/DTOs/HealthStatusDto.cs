namespace MarketData.Api.Domain.DTOs;

/// <summary>
/// Health check response for core dependencies.
/// </summary>
public class HealthStatusDto
{
    /// <summary>
    /// Overall status (e.g. "ok" / "degraded" / "unhealthy").
    /// </summary>
    public string Status { get; set; } = "unhealthy";

    /// <summary>
    /// True if the database dependency is reachable.
    /// </summary>
    public bool DatabaseOk { get; set; }

    /// <summary>
    /// True if the external market data dependency is reachable.
    /// </summary>
    public bool ExternalMarketOk { get; set; }

    /// <summary>
    /// Optional message describing external market status/failure.
    /// </summary>
    public string? ExternalMarketMessage { get; set; }
}
