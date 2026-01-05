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
        // DB check (never throw)
        // --------------------
        try
        {
            dto.DatabaseOk = await _db.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed.");
            dto.DatabaseOk = false;
        }

        // -----------------------------
        // External market check (guard)
        // -----------------------------
        // If not configured, don't even attempt PingAsync (prevents CI/Azure failures).
        if (!TryGetValidExternalBaseUri(_externalSettings.BaseUrl, out var baseUri))
        {
            dto.ExternalMarketOk = false;
            dto.ExternalMarketMessage = "External market not configured.";
        }
        else
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await _marketDataClient.PingAsync(cts.Token);
                dto.ExternalMarketOk = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "External market health check failed for {BaseUrl}", baseUri);
                dto.ExternalMarketOk = false;
                dto.ExternalMarketMessage = ex.Message;
            }
        }

        dto.Status = (dto.DatabaseOk && dto.ExternalMarketOk) ? "ok" : "degraded";
        return dto;
    }

    private static bool TryGetValidExternalBaseUri(string? baseUrl, out Uri? uri)
    {
        uri = null;

        if (string.IsNullOrWhiteSpace(baseUrl))
            return false;

        // Must be absolute + http/https.
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var parsed))
            return false;

        if (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps)
            return false;

        uri = parsed;
        return true;
    }
}
