using MarketData.Api.Domain.DTOs;

namespace MarketData.Api.Services;

public interface IQuoteService
{
    Task<QuoteResponseDto> GetQuoteAsync(string symbol, bool forceRefresh, CancellationToken ct = default);
}
