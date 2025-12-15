namespace MarketData.Api.Common.Errors;

/// <summary>
/// Exception type for controlled API errors (mapped to HTTP responses by middleware).
/// </summary>
public class ApiException : Exception
{
    public ApiException(ErrorCodes errorCode, int statusCode, string message, string? details = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Details = details;
    }

    /// <summary>
    /// Machine-readable error code.
    /// </summary>
    public ErrorCodes ErrorCode { get; }

    /// <summary>
    /// HTTP status code to return.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// Optional extra info (safe for clients).
    /// </summary>
    public string? Details { get; }

    // Convenience factories (optional but nice)
    public static ApiException Unauthorized(string message = "Unauthorized.", string? details = null)
        => new(ErrorCodes.Unauthorized, 401, message, details);

    public static ApiException NotFound(string message = "Not found.", string? details = null)
        => new(ErrorCodes.NotFound, 404, message, details);

    public static ApiException Validation(string message = "Validation error.", string? details = null)
        => new(ErrorCodes.ValidationError, 400, message, details);

    public static ApiException ExternalFailure(string message = "External service failure.", string? details = null)
        => new(ErrorCodes.ExternalServiceFailure, 502, message, details);
}
