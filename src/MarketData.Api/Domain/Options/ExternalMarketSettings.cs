// src/MarketData.Api/Domain/Options/ExternalMarketSettings.cs
namespace MarketData.Api.Domain.Options;

/// <summary>
/// Configuration for the external market data provider.
/// </summary>
public sealed class ExternalMarketSettings
{
    /// <summary>
    /// Base URL of the external market data API (e.g. https://api.example.com/).
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// API key used to authenticate with the external provider.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Timeout in seconds for outbound HTTP calls to the external provider.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;
}
