using MarketData.Api.Common.Errors;
using MarketData.Api.Domain.DTOs;
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

    public async Task<QuoteResponseDto> GetQuoteAsync(string symbol, bool forceRefresh, CancellationToken ct = default)
    {
        var normalized = symbol.Trim().ToUpperInvariant();

        // 1) DB-first unless forceRefresh
        if (!forceRefresh)
        {
            var existing = await _quoteRepository.GetLatestBySymbolAsync(normalized);

            if (existing is not null)
            {
                var ageSeconds = (DateTime.UtcNow - existing.LastUpdatedUtc).TotalSeconds;
                var isFresh = ageSeconds <= _cacheSettings.StaleAfterSeconds;

                if (isFresh)
                {
                    return new QuoteResponseDto
                    {
                        Id = existing.Id,
                        Symbol = existing.Symbol,
                        Price = existing.Price,
                        Currency = existing.Currency,
                        LastUpdatedUtc = existing.LastUpdatedUtc,
                        Source = existing.Source
                    };
                }
            }
        }

        // 2) External fetch only when needed
        Domain.DTOs.External.MarketQuoteDto external;
        try
        {
            // Your interface has no CancellationToken here.
            external = await _marketDataClient.GetLatestQuoteAsync(normalized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External quote fetch failed for {Symbol}", normalized);

            // Use your real ApiException signature (ErrorCodes first, then status code).
            throw new ApiException(
                ErrorCodes.ExternalServiceFailure,
                StatusCodes.Status503ServiceUnavailable,
                "External service call failed.",
                details: "Failed to fetch latest quote from external provider.");
        }

        // 3) Upsert into DB (preserve CreatedAtUtc if row exists)
        var now = DateTime.UtcNow;
        var existingRow = await _quoteRepository.GetLatestBySymbolAsync(normalized);

        var toSave = existingRow ?? new SymbolQuote
        {
            Symbol = normalized,
            CreatedAtUtc = now
        };

        toSave.Price = external.Price;
        toSave.Currency = external.Currency;
        toSave.Source = "external";
        toSave.LastUpdatedUtc = external.TimestampUtc;
        toSave.UpdatedAtUtc = now;

        var saved = await _quoteRepository.UpsertAsync(toSave);

        return new QuoteResponseDto
        {
            Id = saved.Id,
            Symbol = saved.Symbol,
            Price = saved.Price,
            Currency = saved.Currency,
            LastUpdatedUtc = saved.LastUpdatedUtc,
            Source = saved.Source
        };
    }
}
