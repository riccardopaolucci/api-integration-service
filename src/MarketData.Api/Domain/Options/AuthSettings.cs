namespace MarketData.Api.Domain.Options;

/// <summary>
/// Authentication settings bound from configuration section "Auth".
/// </summary>
public class AuthSettings
{
    /// <summary>
    /// JWT issuer (iss).
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// JWT audience (aud).
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Symmetric signing key used to sign JWTs (dev secret in Development).
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Demo/in-memory user credentials and roles.
    /// </summary>
    public InMemoryUserOptions DemoUser { get; set; } = new();
}
