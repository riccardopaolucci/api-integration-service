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
            var dto = JsonSerializer.Deserialize<MarketQuoteDto>(json, JsonOptions);

            if (dto is null ||
                string.IsNullOrWhiteSpace(dto.Symbol) ||
                dto.Price <= 0 ||
                string.IsNullOrWhiteSpace(dto.Currency) ||
                dto.TimestampUtc == default)
            {
                throw new JsonException("Invalid or incomplete payload.");
            }

            return dto;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid payload from external market API for symbol {Symbol}", symbol);

            throw new ApiException(
                ErrorCodes.ExternalServiceFailure,
                StatusCodes.Status503ServiceUnavailable,
                "Invalid response from external market service.");
        }
    }

    private string BuildUrl(string symbol)
    {
        // Generic shape for now (tests don't care about the exact URL).
        // When you pick a real provider, adjust this and include the API key appropriately.
        return $"quote?symbol={Uri.EscapeDataString(symbol)}&apikey={Uri.EscapeDataString(_settings.ApiKey)}";
    }
}

