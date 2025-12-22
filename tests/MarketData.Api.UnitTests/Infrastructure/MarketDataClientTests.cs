// tests/MarketData.Api.UnitTests/Infrastructure/MarketDataClientTests.cs
using System.Net;
using System.Net.Http;
using System.Text;
using MarketData.Api.Common.Errors;
using MarketData.Api.Domain.DTOs.External;
using MarketData.Api.Domain.Options;
using MarketData.Api.Infrastructure.ExternalMarket;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace MarketData.Api.UnitTests.Infrastructure;

public class MarketDataClientTests
{
    [Fact]
    public async Task GetLatestQuoteAsync_ReturnsMappedQuote_OnSuccess()
    {
        // Arrange
        var json = """
        {
          "symbol": "AAPL",
          "price": 123.45,
          "currency": "USD",
          "timestampUtc": "2025-12-22T00:00:00Z"
        }
        """;

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.com/api/")
        };

        var options = Options.Create(new ExternalMarketSettings
        {
            BaseUrl = "https://example.com/api/",
            ApiKey = "demo-or-dev-key",
            TimeoutSeconds = 10
        });

        var sut = new MarketDataClient(httpClient, options, NullLogger<MarketDataClient>.Instance);

        // Act
        MarketQuoteDto quote = await sut.GetLatestQuoteAsync("AAPL");

        // Assert
        Assert.Equal("AAPL", quote.Symbol);
        Assert.Equal(123.45m, quote.Price);
        Assert.Equal("USD", quote.Currency);
        Assert.Equal(DateTime.Parse("2025-12-22T00:00:00Z").ToUniversalTime(), quote.TimestampUtc);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData((HttpStatusCode)429)]
    public async Task GetLatestQuoteAsync_ThrowsApiException_OnNonSuccessStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(statusCode));

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.com/api/")
        };

        var options = Options.Create(new ExternalMarketSettings
        {
            BaseUrl = "https://example.com/api/",
            ApiKey = "demo-or-dev-key",
            TimeoutSeconds = 10
        });

        var sut = new MarketDataClient(httpClient, options, NullLogger<MarketDataClient>.Instance);

        // Act
        var ex = await Assert.ThrowsAsync<ApiException>(() => sut.GetLatestQuoteAsync("AAPL"));

        // Assert
        Assert.Equal(ErrorCodes.ExternalServiceFailure, ex.ErrorCode);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, ex.StatusCode);
    }

    [Theory]
[InlineData("{ \"symbol\": \"AAPL\" }")] // missing fields
[InlineData("{ not-json")]              // malformed JSON (still valid C# string)
public async Task GetLatestQuoteAsync_ThrowsApiException_OnInvalidPayload(string payload)
{
    // Arrange
    var handler = new FakeHttpMessageHandler(_ =>
        new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        });

    var httpClient = new HttpClient(handler)
    {
        BaseAddress = new Uri("https://example.com/api/")
    };

    var options = Options.Create(new ExternalMarketSettings
    {
        BaseUrl = "https://example.com/api/",
        ApiKey = "demo-or-dev-key",
        TimeoutSeconds = 10
    });

    var sut = new MarketDataClient(httpClient, options, NullLogger<MarketDataClient>.Instance);

    // Act
    var ex = await Assert.ThrowsAsync<ApiException>(() => sut.GetLatestQuoteAsync("AAPL"));

    // Assert
    Assert.Equal(ErrorCodes.ExternalServiceFailure, ex.ErrorCode);
    Assert.Equal(StatusCodes.Status503ServiceUnavailable, ex.StatusCode);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}
