namespace MarketData.Api.Common.Errors;

/// <summary>
/// A controlled exception type used to return consistent API errors.
/// </summary>
public class ApiException : Exception
{
    public ApiException(string errorCode, string message, int statusCode = 400, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Machine-readable error code.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// HTTP status code to return.
    /// </summary>
    public int StatusCode { get; }
}
