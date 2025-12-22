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
    .Get<AuthSettings>()!;

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

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

