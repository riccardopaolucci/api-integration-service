using MarketData.Api.Domain.DTOs;
using MarketData.Api.Domain.Options;
using MarketData.Api.Infrastructure.ExternalMarket;
using MarketData.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MarketData.Api.Services;

public class HealthService : IHealthService
{
    private readonly MarketDataDbContext _db;
    private readonly IMarketDataClient _marketDataClient;
    private readonly ExternalMarketSettings _externalSettings;
    private readonly ILogger<HealthService> _logger;

    public HealthService(
        MarketDataDbContext db,
        IMarketDataClient marketDataClient,
        IOptions<ExternalMarketSettings> externalOptions,
        ILogger<HealthService> logger)
    {
        _db = db;
        _marketDataClient = marketDataClient;
        _externalSettings = externalOptions.Value;
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

        // --------------------
        // DB check
        // --------------------
        try
        {
            // EF InMemory provider doesn't support real connectivity checks.
            var provider = _db.Database.ProviderName ?? string.Empty;
            if (provider.Contains("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                dto.DatabaseOk = true;
            }
            else
            {
                dto.DatabaseOk = await _db.Database.CanConnectAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed.");
            dto.DatabaseOk = false;
        }

        // --------------------
        // External market check
        // --------------------
        try
        {
            // If not configured, don't fail health endpoint (common in CI/prod placeholders).
            if (!Uri.TryCreate(_externalSettings.BaseUrl, UriKind.Absolute, out _))
            {
                dto.ExternalMarketOk = true;
                dto.ExternalMarketMessage = "Skipped: ExternalMarket BaseUrl not configured.";
            }
            else
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await _marketDataClient.PingAsync(cts.Token);
                dto.ExternalMarketOk = true;
            }
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
