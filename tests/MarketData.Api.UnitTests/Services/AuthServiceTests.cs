using System.IdentityModel.Tokens.Jwt;
using MarketData.Api.Domain.DTOs;
using MarketData.Api.Domain.Options;
using MarketData.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;
using MarketData.Api.Common.Errors;


namespace MarketData.Api.UnitTests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsValid()
    {
        // Arrange
        var settings = CreateAuthSettings();
        var sut = CreateSut(settings);

        var request = new LoginRequest
        {
            Username = "demo",
            Password = "password123"
        };

        // Act
        var response = await sut.LoginAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));

        // Optional: decode JWT (no signature validation yet, just reading claims)
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(response.Token);

        // sub claim
        Assert.Equal("demo", jwt.Subject);

        // role claim (AuthService should include one)
        var role = jwt.Claims.FirstOrDefault(c => c.Type is "role" or "roles" || c.Type.EndsWith("/role"))?.Value
                   ?? jwt.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
        Assert.False(string.IsNullOrWhiteSpace(role));
        Assert.Equal("Admin", role);

        // exp should be in the future (ValidTo is derived from exp)
        Assert.True(jwt.ValidTo > DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenCredentialsInvalid()
    {
        // Arrange
        var settings = CreateAuthSettings();
        var sut = CreateSut(settings);

        var request = new LoginRequest
        {
            Username = "demo",
            Password = "wrong-password"
        };

        // Act + Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => sut.LoginAsync(request));
        Assert.Equal(ErrorCodes.Unauthorized, ex.ErrorCode);
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenRequestInvalid()
    {
        // Arrange
        var settings = CreateAuthSettings();
        var sut = CreateSut(settings);

        var request = new LoginRequest
        {
            Username = "",
            Password = "password123"
        };

        // Act + Assert
        await Assert.ThrowsAsync<ArgumentException>(() => sut.LoginAsync(request));
    }

    // -----------------------
    // Test helpers
    // -----------------------

    private static IAuthService CreateSut(AuthSettings settings)
    {
        var opts = Options.Create(settings);
        return new AuthService(opts, NullLogger<AuthService>.Instance);
    }

    private static AuthSettings CreateAuthSettings()
    {
        return new AuthSettings
        {
            Issuer = "MarketData.Api",
            Audience = "MarketData.Clients",
            SigningKey = "super-long-dev-secret-key-change-later",
            DemoUser = new InMemoryUserOptions
            {
                Username = "demo",
                Password = "password123",
                Role = "Admin"
            }
        };
    }
}
