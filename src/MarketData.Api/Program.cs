using MarketData.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using MarketData.Api.Repositories;
using MarketData.Api.Services;
using System.Reflection;
using MarketData.Api.Domain.Options;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MarketData.Api.Middleware;
using MarketData.Api.Common.Validation;
using MarketData.Api.Infrastructure.ExternalMarket;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Services
// --------------------
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

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
// DbContext
// --------------------
builder.Services.AddDbContext<MarketDataDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHealthService, HealthService>();

// Options
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));
builder.Services.Configure<ExternalMarketSettings>(builder.Configuration.GetSection("ExternalMarket"));

// Typed HttpClient
builder.Services.AddHttpClient<IMarketDataClient, MarketDataClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ExternalMarketSettings>>().Value;

    // Avoid throwing if BaseUrl is blank; client can still exist for tests/health
    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    }

    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

// --------------------
// Auth (JWT)
// --------------------
var authSettings = builder.Configuration.GetSection("Auth").Get<AuthSettings>();

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
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
}

// --------------------
// Swagger
// --------------------
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// --------------------
// Middleware
// --------------------
app.UseExceptionHandling();
app.UseCors("FrontendCors");

// Swagger on anything except Tests 
if (!app.Environment.IsEnvironment("Test"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html", permanent: true));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --------------------
// Migrations / Seed
// --------------------
// Don’t run in integration tests, and don’t run if no DB configured
var connString = builder.Configuration.GetConnectionString("Default");
var canRunDbBoot = !app.Environment.IsEnvironment("Test") && !string.IsNullOrWhiteSpace(connString);

if (canRunDbBoot)
{
    app.ApplyMigrations();
    await app.SeedAsync();
}

app.Run();

public partial class Program { }
