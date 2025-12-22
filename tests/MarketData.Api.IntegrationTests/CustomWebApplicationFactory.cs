using System.Net;
using MarketData.Api.Domain.DTOs.External;
using MarketData.Api.Infrastructure.ExternalMarket;
using MarketData.Api.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MarketData.Api.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            // -----------------------------
            // Swap DB -> EF InMemory
            // -----------------------------
            services.RemoveAll<DbContextOptions<MarketDataDbContext>>();

            services.AddDbContext<MarketDataDbContext>(options =>
            {
                options.UseInMemoryDatabase("MarketDataTestDb");
            });

            // Ensure DB is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MarketDataDbContext>();
            db.Database.EnsureCreated();

            // -----------------------------
            // Swap external market client -> fake
            // -----------------------------
            services.RemoveAll<IMarketDataClient>();
            services.AddSingleton<IMarketDataClient, FakeMarketDataClient>();
        });
    }

    private sealed class FakeMarketDataClient : IMarketDataClient
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

        // If your interface has a signature without optional token, keep BOTH overloads:
        // public Task PingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}


