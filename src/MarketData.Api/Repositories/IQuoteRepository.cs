using MarketData.Api.Domain.Entities;

namespace MarketData.Api.Repositories;

/// <summary>
/// Repository abstraction for reading and writing symbol quotes.
/// </summary>
public interface IQuoteRepository
{
    /// <summary>
    /// Gets the latest cached quote for a symbol, or null if none exists.
    /// </summary>
    Task<SymbolQuote?> GetLatestBySymbolAsync(string symbol);

    /// <summary>
    /// Inserts or updates a quote record and returns the stored entity.
    /// </summary>
    Task<SymbolQuote> UpsertAsync(SymbolQuote quote);
}
