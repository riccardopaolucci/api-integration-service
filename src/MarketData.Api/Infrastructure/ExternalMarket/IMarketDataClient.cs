using MarketData.Api.Domain.DTOs.External;

namespace MarketData.Api.Infrastructure.ExternalMarket;

public interface IMarketDataClient
{
    Task<MarketQuoteDto> GetLatestQuoteAsync(string symbol);
}
