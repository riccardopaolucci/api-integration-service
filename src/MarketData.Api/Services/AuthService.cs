using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarketData.Api.Domain.DTOs;
using MarketData.Api.Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MarketData.Api.Common.Errors;

namespace MarketData.Api.Services;

/// <summary>
/// Default authentication service that validates credentials and issues JWTs.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AuthSettings _settings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IOptions<AuthSettings> options, ILogger<AuthService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new ArgumentException("Username is required.", nameof(request.Username));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.", nameof(request.Password));
        }

        var demo = _settings.DemoUser;

        // Day 2: plain comparison (upgrade to hashing later)
        // TODO: Replace plaintext comparison with secure hashing (e.g., PBKDF2/BCrypt/Argon2).
        var valid =
            string.Equals(request.Username, demo.Username, StringComparison.Ordinal) &&
            string.Equals(request.Password, demo.Password, StringComparison.Ordinal);

        if (!valid)
        {
            _logger.LogWarning("Invalid login attempt for username '{Username}'.", request.Username);
            throw ApiException.Unauthorized("Invalid username or password.");

        }

        var nowUtc = DateTime.UtcNow;
        var expiresUtc = nowUtc.AddMinutes(60);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, demo.Username),
            new(JwtRegisteredClaimNames.UniqueName, demo.Username),
            new(ClaimTypes.Name, demo.Username)
        };

        // role(s)
        if (!string.IsNullOrWhiteSpace(demo.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, demo.Role));
            // also include "role" to satisfy simple JWT consumers
            claims.Add(new Claim("role", demo.Role));
        }

        if (demo.Roles is { Length: > 0 })
        {
            foreach (var r in demo.Roles.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                claims.Add(new Claim(ClaimTypes.Role, r));
                claims.Add(new Claim("role", r));
            }
        }

        var keyBytes = Encoding.UTF8.GetBytes(_settings.SigningKey);
        var signingKey = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expiresUtc,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var response = new LoginResponse
        {
            Token = tokenString,
            ExpiresAtUtc = expiresUtc,
            Username = demo.Username,
            Roles = demo.Roles ?? (demo.Role is null ? null : new[] { demo.Role })
        };

        return Task.FromResult(response);
    }
}
