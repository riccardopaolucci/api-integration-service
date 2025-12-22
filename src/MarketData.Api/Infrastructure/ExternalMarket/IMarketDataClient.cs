using MarketData.Api.Domain.DTOs.External;

namespace MarketData.Api.Infrastructure.ExternalMarket;

public interface IMarketDataClient
{
    Task<MarketQuoteDto> GetLatestQuoteAsync(string symbol);

    /// <summary>
    /// Checks whether the external market API is reachable (network/TLS/host).
    /// This does not validate payload mapping.
    /// </summary>
    Task PingAsync(CancellationToken cancellationToken = default);
}

