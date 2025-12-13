using MarketData.Api.Domain.DTOs;
using MarketData.Api.Repositories;

namespace MarketData.Api.Services;

/// <summary>
/// Handles retrieval and caching rules for market data quotes.
/// </summary>
public class QuoteService : IQuoteService
{
    private readonly IQuoteRepository _quoteRepository;

    public QuoteService(IQuoteRepository quoteRepository)
    {
        _quoteRepository = quoteRepository;
    }

    public Task<QuoteResponseDto> GetQuoteAsync(string symbol, bool forceRefresh = false)
        => throw new NotImplementedException();
}
