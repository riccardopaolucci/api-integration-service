using System;
using System.Threading.Tasks;
using MarketData.Api.Domain.Entities;
using MarketData.Api.Repositories;
using MarketData.Api.Services;
using Moq;
using Xunit;

namespace MarketData.Api.UnitTests.Services;

public class QuoteServiceTests
{
    private readonly Mock<IQuoteRepository> _quoteRepositoryMock;

    public QuoteServiceTests()
    {
        _quoteRepositoryMock = new Mock<IQuoteRepository>();
    }

    [Fact]
    public async Task GetQuoteAsync_UsesCachedQuote_WhenFreshAndNotForceRefresh()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var freshQuote = new SymbolQuote
        {
            Id = 1,
            Symbol = "AAPL",
            Price = 100m,
            Currency = "USD",
            LastUpdatedUtc = now.AddSeconds(-10),
            Source = "cache",
            CreatedAtUtc = now.AddMinutes(-1),
            UpdatedAtUtc = now.AddSeconds(-10)
        };

        _quoteRepositoryMock
            .Setup(r => r.GetLatestBySymbolAsync("AAPL"))
            .ReturnsAsync(freshQuote);

        var service = new QuoteService(_quoteRepositoryMock.Object);

        // Act
        var result = await service.GetQuoteAsync("AAPL", forceRefresh: false);

        // Assert
        Assert.Equal("AAPL", result.Symbol);
        Assert.Equal(100m, result.Price);
        Assert.Equal("USD", result.Currency);
        Assert.Equal("cache", result.Source);

        _quoteRepositoryMock.Verify(r => r.GetLatestBySymbolAsync("AAPL"), Times.Once);
    }

    [Fact]
    public async Task GetQuoteAsync_Throws_WhenSymbolIsNullOrEmpty()
    {
        var service = new QuoteService(_quoteRepositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetQuoteAsync("", false));
    }
}
