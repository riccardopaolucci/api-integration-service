using MarketData.Api.Domain.DTOs;
using MarketData.Api.Repositories;

namespace MarketData.Api.Services;

/// <summary>
/// Handles retrieval and caching rules for market data quotes.
/// </summary>
public class QuoteService : IQuoteService
{
    private readonly IQuoteRepository _quoteRepository;

    // Simple TTL for "fresh" cache. Adjust later (or move to config).
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Creates a new <see cref="QuoteService"/>.
    /// </summary>
    public QuoteService(IQuoteRepository quoteRepository)
    {
        _quoteRepository = quoteRepository;
    }

    /// <summary>
    /// Gets a quote for the given symbol, using cached data where possible.
    /// </summary>
    public async Task<QuoteResponseDto> GetQuoteAsync(string symbol, bool forceRefresh = false)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol is required.", nameof(symbol));
        }

        var normalizedSymbol = symbol.Trim().ToUpperInvariant();

        var cached = await _quoteRepository.GetLatestBySymbolAsync(normalizedSymbol);

        if (cached is not null && !forceRefresh)
        {
            var age = DateTime.UtcNow - cached.LastUpdatedUtc;

            if (age <= CacheTtl)
            {
                return new QuoteResponseDto
                {
                    Id = cached.Id,
                    Symbol = cached.Symbol,
                    Price = cached.Price,
                    Currency = cached.Currency,
                    LastUpdatedUtc = cached.LastUpdatedUtc,
                    Source = cached.Source
                };
            }
        }

        // External fetch not implemented yet.
        // For now: if we have a cached value (even if stale), return it.
        if (cached is not null)
        {
            return new QuoteResponseDto
            {
                Id = cached.Id,
                Symbol = cached.Symbol,
                Price = cached.Price,
                Currency = cached.Currency,
                LastUpdatedUtc = cached.LastUpdatedUtc,
                Source = cached.Source
            };
        }

        throw new KeyNotFoundException($"No quote found for symbol '{normalizedSymbol}'.");
    }
}
