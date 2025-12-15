namespace MarketData.Api.Common.Errors;

/// <summary>
/// Standard error codes returned by the API.
/// </summary>
public enum ErrorCodes
{
    ValidationError,
    Unauthorized,
    NotFound,
    ExternalServiceFailure,
    Unknown
}
