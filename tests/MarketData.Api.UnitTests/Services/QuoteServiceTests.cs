using MarketData.Api.Common.Errors;
using MarketData.Api.Domain.DTOs.External;
using MarketData.Api.Domain.Entities;
using MarketData.Api.Domain.Options;
using MarketData.Api.Infrastructure.ExternalMarket;
using MarketData.Api.Repositories;
using MarketData.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MarketData.Api.UnitTests.Services;

public class QuoteServiceTests
{
    [Fact]
    public async Task GetQuoteAsync_UsesCached_WhenFresh()
    {
        // Arrange
        var repo = new Mock<IQuoteRepository>();
        var client = new Mock<IMarketDataClient>();

        var cacheSettings = Options.Create(new CacheSettings { StaleAfterSeconds = 60 });

        var cached = new SymbolQuote
        {
            Symbol = "AAPL",
            Price = 100m,
            Currency = "USD",
            LastUpdatedUtc = DateTime.UtcNow.AddSeconds(-30)
        };

        repo.Setup(r => r.GetLatestBySymbolAsync("AAPL"))
            .ReturnsAsync(cached);

        var sut = new QuoteService(repo.Object, client.Object, cacheSettings, NullLogger<QuoteService>.Instance);

        // Act
        var result = await sut.GetQuoteAsync("AAPL", forceRefresh: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("AAPL", result.Symbol);
        Assert.Equal(100m, result.Price);
        Assert.Equal("cache", result.Source);

        client.Verify(c => c.GetLatestQuoteAsync(It.IsAny<string>()), Times.Never);
        repo.Verify(r => r.UpsertAsync(It.IsAny<SymbolQuote>()), Times.Never);
    }

    [Fact]
    public async Task GetQuoteAsync_CallsExternal_WhenStale()
    {
        // Arrange
        var repo = new Mock<IQuoteRepository>();
        var client = new Mock<IMarketDataClient>();

        var cacheSettings = Options.Create(new CacheSettings { StaleAfterSeconds = 60 });

        var cached = new SymbolQuote
        {
            Symbol = "AAPL",
            Price = 100m,
            Currency = "USD",
            LastUpdatedUtc = DateTime.UtcNow.AddSeconds(-120) // stale
        };

        repo.Setup(r => r.GetLatestBySymbolAsync("AAPL"))
            .ReturnsAsync(cached);

        var external = new MarketQuoteDto
        {
            Symbol = "AAPL",
            Price = 200m,
            Currency = "USD",
            TimestampUtc = DateTime.UtcNow
        };

        client.Setup(c => c.GetLatestQuoteAsync("AAPL"))
              .ReturnsAsync(external);

        var sut = new QuoteService(repo.Object, client.Object, cacheSettings, NullLogger<QuoteService>.Instance);

        // Act
        var result = await sut.GetQuoteAsync("AAPL", forceRefresh: false);

        // Assert
        client.Verify(c => c.GetLatestQuoteAsync("AAPL"), Times.Once);

        repo.Verify(r => r.UpsertAsync(It.Is<SymbolQuote>(q =>
            q.Symbol == "AAPL" &&
            q.Price == 200m &&
            q.Currency == "USD")), Times.Once);

        Assert.Equal(200m, result.Price);
        Assert.Equal("external", result.Source);
    }

    [Fact]
    public async Task GetQuoteAsync_CallsExternal_WhenMissing()
    {
        // Arrange
        var repo = new Mock<IQuoteRepository>();
        var client = new Mock<IMarketDataClient>();

        var cacheSettings = Options.Create(new CacheSettings { StaleAfterSeconds = 60 });

        repo.Setup(r => r.GetLatestBySymbolAsync("AAPL"))
            .ReturnsAsync((SymbolQuote?)null);

        var external = new MarketQuoteDto
        {
            Symbol = "AAPL",
            Price = 222m,
            Currency = "USD",
            TimestampUtc = DateTime.UtcNow
        };

        client.Setup(c => c.GetLatestQuoteAsync("AAPL"))
              .ReturnsAsync(external);

        var sut = new QuoteService(repo.Object, client.Object, cacheSettings, NullLogger<QuoteService>.Instance);

        // Act
        var result = await sut.GetQuoteAsync("AAPL", forceRefresh: false);

        // Assert
        client.Verify(c => c.GetLatestQuoteAsync("AAPL"), Times.Once);
        repo.Verify(r => r.UpsertAsync(It.IsAny<SymbolQuote>()), Times.Once);

        Assert.Equal(222m, result.Price);
        Assert.Equal("external", result.Source);
    }

    [Fact]
    public async Task GetQuoteAsync_ThrowsApiException_WhenExternalFails()
    {
        // Arrange
        var repo = new Mock<IQuoteRepository>();
        var client = new Mock<IMarketDataClient>();

        var cacheSettings = Options.Create(new CacheSettings { StaleAfterSeconds = 60 });

        repo.Setup(r => r.GetLatestBySymbolAsync("AAPL"))
            .ReturnsAsync((SymbolQuote?)null);

        client.Setup(c => c.GetLatestQuoteAsync("AAPL"))
              .ThrowsAsync(new HttpRequestException("boom"));

        var sut = new QuoteService(repo.Object, client.Object, cacheSettings, NullLogger<QuoteService>.Instance);

        // Act
        var ex = await Assert.ThrowsAsync<ApiException>(() => sut.GetQuoteAsync("AAPL", forceRefresh: false));

        // Assert
        Assert.Equal(ErrorCodes.ExternalServiceFailure, ex.ErrorCode);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, ex.StatusCode);
    }
}
