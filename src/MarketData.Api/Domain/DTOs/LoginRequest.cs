using System.ComponentModel.DataAnnotations;

namespace MarketData.Api.Domain.DTOs;

/// <summary>
/// Request payload for authenticating a user and generating a JWT.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username for login.
    /// </summary>
    [Required]
    [MinLength(3)]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for login.
    /// </summary>
    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
