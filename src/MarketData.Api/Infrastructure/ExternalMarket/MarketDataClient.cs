using System.Globalization;
using System.Text.Json;
using MarketData.Api.Common.Errors;
using MarketData.Api.Domain.DTOs.External;
using MarketData.Api.Domain.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MarketData.Api.Infrastructure.ExternalMarket;

/// <summary>
/// Real HTTP client for fetching market data from an external provider.
/// Currently implemented for Alpha Vantage (GLOBAL_QUOTE).
/// </summary>
public sealed class MarketDataClient : IMarketDataClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ExternalMarketSettings _settings;
    private readonly ILogger<MarketDataClient> _logger;

    public MarketDataClient(
        HttpClient httpClient,
        IOptions<ExternalMarketSettings> options,
        ILogger<MarketDataClient> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<MarketQuoteDto> GetLatestQuoteAsync(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol is required.", nameof(symbol));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(symbol));

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.SendAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call external market API for symbol {Symbol}", symbol);

            throw new ApiException(
                ErrorCodes.ExternalServiceFailure,
                StatusCodes.Status503ServiceUnavailable,
                "External market data service is unavailable.");
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "External market API returned {StatusCode} for symbol {Symbol}",
                response.StatusCode,
                symbol);

            throw new ApiException(
                ErrorCodes.ExternalServiceFailure,
                StatusCodes.Status503ServiceUnavailable,
                $"External service returned status code {(int)response.StatusCode}.");
        }

        var json = await response.Content.ReadAsStringAsync();

        try
        {
            // Alpha Vantage returns:
            // { "Global Quote": { "01. symbol": "...", "05. price": "...", "07. latest trading day": "..." ... } }
            // It can also return { "Note": "...rate limit..." } or { "Information": "..." } with 200 OK.
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("Root JSON is not an object.");
            }

            if (doc.RootElement.TryGetProperty("Note", out var noteEl))
            {
                var note = noteEl.GetString();
                _logger.LogWarning("Alpha Vantage rate limit hit. Note: {Note}", note);

                throw new ApiException(
                    ErrorCodes.ExternalServiceFailure,
                    StatusCodes.Status503ServiceUnavailable,
                    "External market data provider rate-limited the request. Try again shortly.");
            }

            if (doc.RootElement.TryGetProperty("Information", out var infoEl))
            {
                var info = infoEl.GetString();
                _logger.LogWarning("Alpha Vantage information response. Info: {Info}", info);

                throw new ApiException(
                    ErrorCodes.ExternalServiceFailure,
                    StatusCodes.Status503ServiceUnavailable,
                    "External market data provider did not return quote data.");
            }

            if (!doc.RootElement.TryGetProperty("Global Quote", out var quoteEl) ||
                quoteEl.ValueKind != JsonValueKind.Object)
            {
                throw new JsonException("Missing 'Global Quote' object.");
            }

            var parsedSymbol = GetString(quoteEl, "01. symbol");
            var priceStr = GetString(quoteEl, "05. price");
            var tradingDayStr = GetString(quoteEl, "07. latest trading day"); // optional-ish

            if (string.IsNullOrWhiteSpace(parsedSymbol))
            {
                throw new JsonException("Missing '01. symbol'.");
            }

            if (string.IsNullOrWhiteSpace(priceStr) ||
                !decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) ||
                price <= 0)
            {
                throw new JsonException("Invalid '05. price'.");
            }

            // Alpha Vantage Global Quote doesn't reliably include currency.
            // Default to USD to satisfy downstream validation/contracts.
            var currency = "USD";

            // Best-effort timestamp: if we can parse the latest trading day, use midnight UTC for that day,
            // otherwise just use "now" so caching still functions.
            var timestampUtc = TryParseTradingDayUtc(tradingDayStr) ?? DateTime.UtcNow;

            return new MarketQuoteDto
            {
                Symbol = parsedSymbol,
                Price = price,
                Currency = currency,
                TimestampUtc = timestampUtc
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Invalid payload from external market API for symbol {Symbol}. Raw: {Json}",
                symbol,
                json);

            throw new ApiException(
                ErrorCodes.ExternalServiceFailure,
                StatusCodes.Status503ServiceUnavailable,
                "Invalid response from external market service.");
        }
    }

    public async Task PingAsync(CancellationToken cancellationToken = default)
    {
        // Goal: confirm we can reach the host (DNS/TLS/connect) quickly.
        // Accept ANY HTTP status code as "reachable".
        using var request = new HttpRequestMessage(HttpMethod.Get, string.Empty);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "External market API ping failed.");
            throw new ApiException(
                ErrorCodes.ExternalServiceFailure,
                StatusCodes.Status503ServiceUnavailable,
                "External market data provider unreachable.");
        }

        // If we got a response at all, the service is reachable.
        // Do NOT require 2xx because many APIs return 401/403 without an API key.
        _ = response.StatusCode;
    }

    private string BuildUrl(string symbol)
    {
        // Alpha Vantage GLOBAL_QUOTE:
        // https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol=IBM&apikey=demo
        return $"query?function=GLOBAL_QUOTE&symbol={Uri.EscapeDataString(symbol)}&apikey={Uri.EscapeDataString(_settings.ApiKey)}";
    }

    private static string? GetString(JsonElement obj, string propertyName)
    {
        if (!obj.TryGetProperty(propertyName, out var el))
        {
            return null;
        }

        return el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.GetRawText(),
            _ => el.GetRawText()
        };
    }

    private static DateTime? TryParseTradingDayUtc(string? tradingDay)
    {
        if (string.IsNullOrWhiteSpace(tradingDay))
        {
            return null;
        }

        // Alpha Vantage typically returns "YYYY-MM-DD"
        if (DateTime.TryParseExact(
                tradingDay,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dt))
        {
            // Make it explicit UTC (date-only → midnight)
            return DateTime.SpecifyKind(dt.Date, DateTimeKind.Utc);
        }

        return null;
    }
}
