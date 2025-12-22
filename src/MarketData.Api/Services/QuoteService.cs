using MarketData.Api.Common.Errors;
using MarketData.Api.Domain.DTOs;
using MarketData.Api.Domain.DTOs.External;
using MarketData.Api.Domain.Entities;
using MarketData.Api.Domain.Options;
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

    /// <summary>
    /// Creates a new <see cref="QuoteService"/>.
    /// </summary>
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

        if (!forceRefresh && cached is not null && !IsStale(cached))
        {
            return ToDto(cached, source: "cache");
        }

        try
        {
            MarketQuoteDto external = await _marketDataClient.GetLatestQuoteAsync(normalizedSymbol);

            // Map external DTO -> entity
            var now = DateTime.UtcNow;

            var latest = new SymbolQuote
            {
                Symbol = normalizedSymbol,
                Price = external.Price,
                Currency = external.Currency,
                Source = "ExternalMarket",
                LastUpdatedUtc = external.TimestampUtc,
                CreatedAtUtc = cached?.CreatedAtUtc ?? now,
                UpdatedAtUtc = now
            };

            await _quoteRepository.UpsertAsync(latest);

            return ToDto(latest, source: "external");
        }
        catch (ApiException)
        {
            // Preserve upstream error code/status/message from MarketDataClient
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "External market data provider failed for symbol {Symbol}", normalizedSymbol);

            throw new ApiException(
                ErrorCodes.ExternalServiceFailure,
                StatusCodes.Status503ServiceUnavailable,
                "External market data provider unavailable.");
        }
    }

    private bool IsStale(SymbolQuote quote)
    {
        var ageSeconds = (DateTime.UtcNow - quote.LastUpdatedUtc).TotalSeconds;
        return ageSeconds > _cacheSettings.StaleAfterSeconds;
    }

    private static QuoteResponseDto ToDto(SymbolQuote quote, string source)
    {
        return new QuoteResponseDto
        {
            Id = quote.Id,
            Symbol = quote.Symbol,
            Price = quote.Price,
            Currency = quote.Currency,
            LastUpdatedUtc = quote.LastUpdatedUtc,
            Source = source
        };
    }
}
