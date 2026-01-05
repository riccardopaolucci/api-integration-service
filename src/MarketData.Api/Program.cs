using System.Reflection;
using System.Text;
using MarketData.Api.Common.Validation;
using MarketData.Api.Domain.Options;
using MarketData.Api.Infrastructure.ExternalMarket;
using MarketData.Api.Middleware;
using MarketData.Api.Persistence;
using MarketData.Api.Repositories;
using MarketData.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Controllers + Validation
// --------------------
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddEndpointsApiExplorer();

// --------------------
// CORS (Frontend)
// --------------------
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --------------------
// Database
// --------------------
var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<MarketDataDbContext>(options =>
{
    // If the connection string is missing, we still let the app boot so you can see a useful error/health response.
    // Your API endpoints that require DB will fail until you set ConnectionStrings:Default in Azure.
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        options.UseNpgsql(connectionString);
    }
});

// --------------------
// App Services
// --------------------
builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHealthService, HealthService>();

// Options
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));
builder.Services.Configure<ExternalMarketSettings>(builder.Configuration.GetSection("ExternalMarket"));

// --------------------
// External Market HttpClient
// --------------------
builder.Services.AddHttpClient<IMarketDataClient, MarketDataClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ExternalMarketSettings>>().Value;

    // Don’t crash startup if BaseUrl isn’t set yet (common during first Azure deploy).
    if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
    {
        client.BaseAddress = baseUri;
    }

    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 10);
});

// --------------------
// Auth (JWT)
// --------------------
var authSettings = builder.Configuration
    .GetSection("Auth")
    .Get<AuthSettings>();

if (authSettings is not null && !string.IsNullOrWhiteSpace(authSettings.SigningKey))
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.SigningKey));

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = authSettings.Issuer,
                ValidAudience = authSettings.Audience,
                IssuerSigningKey = key
            };
        });

    builder.Services.AddAuthorization();
}
else
{
    // Allows the app to boot (e.g. IntegrationTests / first deploy before secrets are set)
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
}

// --------------------
// Swagger
// --------------------
builder.Services.AddSwaggerGen(options =>
{
    // XML comments (safe if file exists)
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --------------------
// Middleware pipeline (order matters)
// --------------------
app.UseExceptionHandling();

app.UseRouting();

app.UseCors("FrontendCors");

app.UseAuthentication();
app.UseAuthorization();

// Swagger: enable in ALL envs (handy for Azure testing).
// If you want it only in Dev later, wrap in app.Environment.IsDevelopment().
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => Results.Redirect("/swagger", permanent: false));
app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html", permanent: false));

// Health endpoint (public)
app.MapGet("/healthz", async (IHealthService healthService) =>
{
    // Whatever your HealthService returns, just pass it through.
    // If your IHealthService returns a DTO, this will JSON it.
    var result = await healthService.GetHealthAsync();
    return Results.Ok(result);
}).AllowAnonymous();

// Controllers
app.MapControllers();

// DB migrations + seed (don’t take down the whole app if config isn’t ready yet)
try
{
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        app.ApplyMigrations();
        await app.SeedAsync();
    }
    else
    {
        app.Logger.LogWarning("ConnectionStrings:Default is not set. Skipping migrations/seed.");
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database migration/seed failed. The app will still run, but DB-backed endpoints will fail until fixed.");
}

app.Run();

public partial class Program { }
