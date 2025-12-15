using MarketData.Api.Domain.DTOs;
using MarketData.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketData.Api.Controllers;

/// <summary>
/// Handles authentication requests.
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Validates credentials and returns a JWT.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>JWT token and authentication metadata.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }
}
