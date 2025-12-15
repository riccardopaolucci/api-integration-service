namespace MarketData.Api.Domain.Options;

/// <summary>
/// Configuration for the demo/in-memory user used by the AuthService.
/// Bound from configuration (e.g., appsettings.Development.json).
/// </summary>
public class InMemoryUserOptions
{
    /// <summary>
    /// Demo username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Plaintext password (dev only). Use <see cref="PasswordHash"/> in production.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Password hash for the demo user (preferred). If set, AuthService should verify against this.
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Single role for the demo user (simple case).
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Optional multiple roles for the demo user (future-proofing).
    /// </summary>
    public string[]? Roles { get; set; }
}
