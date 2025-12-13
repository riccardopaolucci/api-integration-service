using MarketData.Api.Domain.Entities;

namespace MarketData.Api.Repositories;

/// <summary>
/// Repository implementation for symbol quotes.
/// NOTE: Placeholder for Day 1; persistence implementation comes later.
/// </summary>
public class QuoteRepository : IQuoteRepository
{
    public Task<SymbolQuote?> GetLatestBySymbolAsync(string symbol)
        => throw new NotImplementedException();

    public Task<SymbolQuote> UpsertAsync(SymbolQuote quote)
        => throw new NotImplementedException();
}
