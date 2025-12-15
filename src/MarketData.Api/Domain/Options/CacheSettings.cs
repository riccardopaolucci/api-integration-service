namespace MarketData.Api.Domain.Options;

/// <summary>
/// Cache settings for quote freshness/staleness rules.
/// </summary>
public class CacheSettings
{
    /// <summary>
    /// How many seconds until a cached quote is considered stale.
    /// </summary>
    public int StaleAfterSeconds { get; set; } = 60;
}
