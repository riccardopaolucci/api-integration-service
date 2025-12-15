using MarketData.Api.Domain.DTOs;

namespace MarketData.Api.Services;

/// <summary>
/// Handles authentication and JWT token creation for API clients.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Validates credentials and returns a signed JWT if valid.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>A JWT token and related authentication metadata.</returns>
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
