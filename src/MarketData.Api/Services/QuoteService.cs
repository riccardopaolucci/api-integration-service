using MarketData.Api.Common.Errors;
using MarketData.Api.Domain.DTOs;
using MarketData.Api.Domain.Entities;
using MarketData.Api.Domain.Options;
using MarketData.Api.Infrastructure.ExternalMarket;
using MarketData.Api.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net;
using MarketData.Api.Domain.DTOs.External;



namespace MarketData.Api.Services;

/// <summary>
/// Handles retrieval and caching rules for market data quotes.
/// </summary>
public class QuoteService : IQuoteService
{
    private readonly IQuoteRepository _quoteRepository;
    private readonly IMarketDataClient _marketDataClient;
    private readonly CacheSettings _cacheSettings;
    private readonly ILogger<QuoteService> _logger;

    public QuoteService(
        IQuoteRepository quoteRepository,
        IMarketDataClient marketDataClient,
        IOptions<CacheSettings> cacheOptions,
        ILogger<QuoteService> logger)
    {
        _quoteRepository = quoteRepository;
        _marketDataClient = marketDataClient;
        _cacheSettings = cacheOptions.Value;
        _logger = logger;
    }

    public async Task<QuoteResponseDto> GetQuoteAsync(
        string symbol,
        bool forceRefresh,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol is required.", nameof(symbol));

        var cached = await _quoteRepository.GetLatestBySymbolAsync(symbol);

        var isMissing = cached is null;

        var staleAfterSeconds = _cacheSettings.StaleAfterSeconds;

        var isStale = !isMissing &&
                    (DateTime.UtcNow - cached!.LastUpdatedUtc).TotalSeconds > staleAfterSeconds;

        // -----------------------------
        // Use cache if fresh
        // -----------------------------
        if (!forceRefresh && !isMissing && !isStale)
        {
            return new QuoteResponseDto
            {
                Symbol = cached!.Symbol,
                Price = cached.Price,
                Currency = cached.Currency,
                LastUpdatedUtc = cached.LastUpdatedUtc,
                Source = "cache" // unit test expects this
            };
        }

        // -----------------------------
        // Fetch from external provider
        // -----------------------------
        MarketQuoteDto external;

        try
        {
            external = await _marketDataClient.GetLatestQuoteAsync(symbol);
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(
                ErrorCodes.ExternalServiceFailure,
                StatusCodes.Status503ServiceUnavailable, // ✅ correct constant
                "External market data provider request failed.",
                ex.Message,
                ex
            );

        }



        var now = DateTime.UtcNow;

        var entity = cached ?? new SymbolQuote
        {
            Symbol = symbol,
            CreatedAtUtc = now
        };

        entity.Price = external.Price;
        entity.Currency = external.Currency;
        entity.LastUpdatedUtc = external.TimestampUtc;
        entity.UpdatedAtUtc = now;
        entity.Source = "external";

        await _quoteRepository.UpsertAsync(entity);

        return new QuoteResponseDto
        {
            Symbol = entity.Symbol,
            Price = entity.Price,
            Currency = entity.Currency,
            LastUpdatedUtc = entity.LastUpdatedUtc,
            Source = entity.Source
        };
    }

}
