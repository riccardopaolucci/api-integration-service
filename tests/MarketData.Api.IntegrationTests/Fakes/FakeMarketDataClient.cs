using MarketData.Api.Domain.DTOs.External;
using MarketData.Api.Infrastructure.ExternalMarket;

namespace MarketData.Api.IntegrationTests.Fakes;

public sealed class FakeMarketDataClient : IMarketDataClient
{
    public Task<MarketQuoteDto> GetLatestQuoteAsync(string symbol)
    {
        return Task.FromResult(new MarketQuoteDto
        {
            Symbol = symbol,
            Price = 1.23m,
            Currency = "USD",
            TimestampUtc = DateTime.UtcNow
        });
    }

    public Task PingAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
