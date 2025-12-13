using MarketData.Api.Domain.DTOs;
using MarketData.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketData.Api.Controllers;

/// <summary>
/// Endpoints for retrieving market quotes by symbol or id.
/// </summary>
[ApiController]
[Route("quotes")]
public class QuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;

    public QuotesController(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    /// <summary>
    /// Gets a quote by symbol, optionally forcing a refresh.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<QuoteResponseDto>> GetBySymbol(
        [FromQuery] string symbol,
        [FromQuery] bool forceRefresh = false)
    {
        var quote = await _quoteService.GetQuoteAsync(symbol, forceRefresh);
        return Ok(quote);
    }
}
