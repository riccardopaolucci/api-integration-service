using MarketData.Api.Domain.DTOs;

namespace MarketData.Api.Services;

/// <summary>
/// Handles retrieval and caching rules for market data quotes.
/// </summary>
public interface IQuoteService
{
    /// <summary>
    /// Gets a quote for the given symbol, using cached data where possible.
    /// </summary>
    Task<QuoteResponseDto> GetQuoteAsync(string symbol, bool forceRefresh = false);
}
