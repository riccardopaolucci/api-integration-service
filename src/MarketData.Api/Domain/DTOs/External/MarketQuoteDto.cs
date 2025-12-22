// src/MarketData.Api/Domain/DTOs/External/MarketQuoteDto.cs
namespace MarketData.Api.Domain.DTOs.External;

/// <summary>
/// Represents the external market data provider's quote payload.
/// </summary>
public sealed class MarketQuoteDto
{
    /// <summary>
    /// The requested symbol (e.g. AAPL, BTC-USD).
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Latest price returned by the external provider.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Currency of the price (e.g. USD).
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of the quote in UTC.
    /// </summary>
    public DateTime TimestampUtc { get; set; }
}
