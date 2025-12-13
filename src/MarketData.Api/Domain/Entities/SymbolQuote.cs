namespace MarketData.Api.Domain.Entities;

/// <summary>
/// Represents a cached market data quote for a given symbol.
/// </summary>
public class SymbolQuote
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime LastUpdatedUtc { get; set; }
    public string? RawPayloadJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
