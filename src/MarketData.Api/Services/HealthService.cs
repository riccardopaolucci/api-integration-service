using MarketData.Api.Domain.DTOs;
using MarketData.Api.Infrastructure.ExternalMarket;
using MarketData.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarketData.Api.Services;

public class HealthService : IHealthService
{
    private readonly MarketDataDbContext _db;
    private readonly IMarketDataClient _marketDataClient;
    private readonly ILogger<HealthService> _logger;

    public HealthService(
        MarketDataDbContext db,
        IMarketDataClient marketDataClient,
        ILogger<HealthService> logger)
    {
        _db = db;
        _marketDataClient = marketDataClient;
        _logger = logger;
    }

    public async Task<HealthStatusDto> GetHealthAsync()
    {
        var dto = new HealthStatusDto
        {
            Status = "degraded",
            DatabaseOk = false,
            ExternalMarketOk = false,
            ExternalMarketMessage = null
        };

        // DB check
        try
        {
            dto.DatabaseOk = await _db.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed.");
            dto.DatabaseOk = false;
        }

        // External market check (fast timeout)
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await _marketDataClient.PingAsync(cts.Token);
            dto.ExternalMarketOk = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "External market health check failed.");
            dto.ExternalMarketOk = false;
            dto.ExternalMarketMessage = ex.Message;
        }

        dto.Status = (dto.DatabaseOk && dto.ExternalMarketOk) ? "ok" : "degraded";
        return dto;
    }
}
