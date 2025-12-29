using MarketData.Api.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MarketData.Api.Persistence;

/// <summary>
/// Seeds initial demo data for development and testing.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(MarketDataDbContext context)
    {
        if (!context.SymbolQuotes.Any())
        {
            var now = DateTime.UtcNow;

            context.SymbolQuotes.AddRange(
                new SymbolQuote
                {
                    Symbol = "AAPL",
                    Price = 150m,
                    Currency = "USD",
                    Source = "seed",
                    LastUpdatedUtc = now,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                },
                new SymbolQuote
                {
                    Symbol = "BTC-USD",
                    Price = 30000m,
                    Currency = "USD",
                    Source = "seed",
                    LastUpdatedUtc = now,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });

            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MarketDataDbContext>();
        await SeedAsync(dbContext);
    }
}
