using MarketData.Api.Domain.Entities;

namespace MarketData.Api.Infrastructure.ExternalMarket;

/// <summary>
/// Abstraction over an external market data provider.
/// </summary>
public interface IMarketDataClient
{
    /// <summary>
    /// Fetches the latest quote for a symbol from the external provider.
    /// </summary>
    Task<SymbolQuote> GetLatestQuoteAsync(string symbol);
}
