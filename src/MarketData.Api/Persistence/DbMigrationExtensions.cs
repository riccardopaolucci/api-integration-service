using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MarketData.Api.Persistence;

/// <summary>
/// Helpers to apply migrations at application startup.
/// </summary>
public static class DbMigrationExtensions
{
    public static void ApplyMigrations(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MarketDataDbContext>();
        dbContext.Database.Migrate();
    }
}
