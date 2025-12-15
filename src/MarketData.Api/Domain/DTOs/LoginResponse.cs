using System.ComponentModel.DataAnnotations;

namespace MarketData.Api.Domain.DTOs;

/// <summary>
/// Response payload containing the generated JWT and related metadata.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// The JWT bearer token to use in the Authorization header.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The UTC timestamp when the token expires (optional).
    /// </summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>
    /// The authenticated username (optional).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Roles associated with the authenticated user (optional).
    /// </summary>
    public string[]? Roles { get; set; }
}
