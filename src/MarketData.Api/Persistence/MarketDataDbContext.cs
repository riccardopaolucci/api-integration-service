using MarketData.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketData.Api.Persistence;

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

