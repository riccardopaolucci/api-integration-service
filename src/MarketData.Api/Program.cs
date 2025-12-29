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

builder.Services.AddDbContext<MarketDataDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
    )
);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHealthService, HealthService>();

// Cache & Auth options
builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection("Cache"));

builder.Services.Configure<AuthSettings>(
    builder.Configuration.GetSection("Auth"));

// External market options
builder.Services.Configure<ExternalMarketSettings>(
    builder.Configuration.GetSection("ExternalMarket"));

// Typed HttpClient for real external market client
builder.Services.AddHttpClient<IMarketDataClient, MarketDataClient>((sp, client) =>
{
    var options = sp
        .GetRequiredService<IOptions<ExternalMarketSettings>>()
        .Value;

    client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

// --------------------
// Auth (JWT)
// --------------------

// Read auth settings for JWT validation
var authSettings = builder.Configuration
    .GetSection("Auth")
    .Get<AuthSettings>();

if (authSettings is not null && !string.IsNullOrWhiteSpace(authSettings.SigningKey))
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(authSettings.SigningKey));

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
    // Allow the app to boot in IntegrationTests even if Auth config isn't present.
    builder.Services.AddAuthentication();
    builder.Services.AddAuthorization();
}

builder.Services.AddSwaggerGen(options =>
{
    // XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // JWT Bearer auth
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapGet("/swagger", () => Results.Redirect("/swagger/index.html", permanent: true));

app.ApplyMigrations();
await app.SeedAsync();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
