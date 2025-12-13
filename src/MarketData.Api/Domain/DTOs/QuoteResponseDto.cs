namespace MarketData.Api.Domain.DTOs;

/// <summary>
/// DTO returned to API clients representing a market quote.
/// </summary>
public class QuoteResponseDto
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime LastUpdatedUtc { get; set; }
    public string Source { get; set; } = string.Empty;
}
