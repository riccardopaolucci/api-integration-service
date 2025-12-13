namespace MarketData.Api.Domain.DTOs;

/// <summary>
/// DTO returned to API clients representing a market quote.
/// </summary>
public class QuoteResponseDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime LastUpdatedUtc { get; set; }
    public bool FromCache { get; set; }
}
