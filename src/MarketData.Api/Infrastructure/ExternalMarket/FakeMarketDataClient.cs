using MarketData.Api.Domain.Entities;

namespace MarketData.Api.Infrastructure.ExternalMarket;

/// <summary>
/// Fake external market data client for development/testing.
/// Always returns a deterministic quote with a current timestamp.
/// </summary>
public class FakeMarketDataClient : IMarketDataClient
{
    public Task<SymbolQuote> GetLatestQuoteAsync(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol is required.", nameof(symbol));
        }

        var quote = new SymbolQuote
        {
            Symbol = symbol.Trim().ToUpperInvariant(),
            Price = 123.45m,
            LastUpdatedUtc = DateTime.UtcNow
        };

        return Task.FromResult(quote);
    }
}
