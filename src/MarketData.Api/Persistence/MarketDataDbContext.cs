using MarketData.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketData.Api.Persistence;

/// <summary>
/// Entity Framework Core DbContext for market data and cached quotes.
/// </summary>
public class MarketDataDbContext : DbContext
{
    public MarketDataDbContext(DbContextOptions<MarketDataDbContext> options)
        : base(options)
    {
    }

    public DbSet<SymbolQuote> SymbolQuotes => Set<SymbolQuote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SymbolQuote>()
            .HasIndex(q => q.Symbol);

        modelBuilder.Entity<SymbolQuote>()
            .HasIndex(q => q.LastUpdatedUtc);
    }
}
