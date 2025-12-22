using MarketData.Api.Domain.Entities;
using MarketData.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MarketData.Api.Repositories;

/// <summary>
/// Repository for reading/writing cached symbol quotes.
/// </summary>
public class QuoteRepository : IQuoteRepository
{
    private readonly MarketDataDbContext _dbContext;

    public QuoteRepository(MarketDataDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SymbolQuote?> GetLatestBySymbolAsync(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return null;
        }

        var normalized = symbol.Trim().ToUpperInvariant();

        return await _dbContext.SymbolQuotes
            .AsNoTracking()
            .Where(q => q.Symbol == normalized)
            .OrderByDescending(q => q.LastUpdatedUtc)
            .FirstOrDefaultAsync();
    }

    public async Task<SymbolQuote> UpsertAsync(SymbolQuote quote)
    {
        if (quote is null) throw new ArgumentNullException(nameof(quote));
        if (string.IsNullOrWhiteSpace(quote.Symbol))
            throw new ArgumentException("Symbol is required.", nameof(quote));

        quote.Symbol = quote.Symbol.Trim().ToUpperInvariant();

        var existing = await _dbContext.SymbolQuotes
            .FirstOrDefaultAsync(q => q.Symbol == quote.Symbol);

        if (existing is null)
        {
            _dbContext.SymbolQuotes.Add(quote);
            await _dbContext.SaveChangesAsync();
            return quote;
        }

        // Update existing cached row
        existing.Price = quote.Price;
        existing.Currency = quote.Currency;
        existing.LastUpdatedUtc = quote.LastUpdatedUtc;

        await _dbContext.SaveChangesAsync();
        return existing;
    }
}
