using System.Net;
using System.Net.Http.Json;
using MarketData.Api.Domain.DTOs;
using MarketData.Api.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MarketData.Api.IntegrationTests;

public class QuotesEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public QuotesEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetQuotes_ReturnsQuote_AndPersistsToDb()
    {
        // Act
        var response = await _client.GetAsync("/quotes?symbol=TEST");

        // Assert HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert payload
        var quote = await response.Content.ReadFromJsonAsync<QuoteResponseDto>();
        Assert.NotNull(quote);
        Assert.Equal("TEST", quote!.Symbol);
        Assert.Equal("USD", quote.Currency);

        // Assert persisted to DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MarketDataDbContext>();

        var exists = db.SymbolQuotes.Any(q => q.Symbol == "TEST");
        Assert.True(exists, "Expected quote to be persisted to the database.");
    }
}
