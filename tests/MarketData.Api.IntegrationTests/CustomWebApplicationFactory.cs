using Microsoft.AspNetCore.Mvc.Testing;

namespace MarketData.Api.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Later: override ConfigureWebHost(...) to swap DB to in-memory, etc.
}
