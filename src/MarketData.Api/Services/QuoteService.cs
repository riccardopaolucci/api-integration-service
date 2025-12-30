using MarketData.Api.Common.Errors;
using MarketData.Api.Domain.DTOs;
using MarketData.Api.Domain.Entities;
using MarketData.Api.Domain.Options;
using MarketData.Api.Domain.DTOs.External;
using MarketData.Api.Infrastructure.ExternalMarket;
using MarketData.Api.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

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
            // If we have *any* cached value, fall back to it instead of failing the whole request.
            // This is especially useful in Development when the external provider is a placeholder.
            if (cached is not null)
            {
                _logger.LogWarning(ex,
                    "External provider failed for {Symbol}. Falling back to cached quote from {LastUpdatedUtc}.",
                    symbol, cached.LastUpdatedUtc);

                return new QuoteResponseDto
                {
                    Symbol = cached.Symbol,
                    Price = cached.Price,
                    Currency = cached.Currency,
                    LastUpdatedUtc = cached.LastUpdatedUtc,
                    Source = cached.Source ?? "cache"
                };
            }

            throw new ApiException(
                ErrorCodes.ExternalServiceFailure,
                StatusCodes.Status503ServiceUnavailable,
                "External market data provider request failed.",
                ex.Message,
                ex
            );
        }
        catch (ApiException ex) when (ex.ErrorCode == ErrorCodes.ExternalServiceFailure)
        {
            // MarketDataClient can throw ApiException for non-success status codes (e.g. 404 from example.com).
            // Fall back to cached if we have it; otherwise propagate as 503.
            if (cached is not null)
            {
                _logger.LogWarning(ex,
                    "External provider returned failure for {Symbol}. Falling back to cached quote from {LastUpdatedUtc}.",
                    symbol, cached.LastUpdatedUtc);

                return new QuoteResponseDto
                {
                    Symbol = cached.Symbol,
                    Price = cached.Price,
                    Currency = cached.Currency,
                    LastUpdatedUtc = cached.LastUpdatedUtc,
                    Source = cached.Source ?? "cache"
                };
            }

            // Re-throw if there's nothing to fall back to
            throw;
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
