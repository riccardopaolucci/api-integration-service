using Microsoft.EntityFrameworkCore;

namespace MarketData.Api.Persistence;

/// <summary>
/// Helpers to apply migrations and seed data at application startup.
/// </summary>
public static class DbMigrationExtensions
{
    public static void ApplyMigrations(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MarketDataDbContext>();

        // InMemory provider doesn't support Migrate()
        if (dbContext.Database.IsRelational())
        {
            dbContext.Database.Migrate();
        }
        else
        {
            dbContext.Database.EnsureCreated();
        }
    }
}
