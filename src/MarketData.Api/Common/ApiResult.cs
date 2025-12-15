namespace MarketData.Api.Common;

/// <summary>
/// Standard response wrapper for consistent API responses (optional usage).
/// </summary>
public class ApiResult<T>
{
    /// <summary>
    /// Indicates whether the request succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error code when <see cref="Success"/> is false.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Human-readable error message when <see cref="Success"/> is false.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Payload when <see cref="Success"/> is true.
    /// </summary>
    public T? Data { get; set; }

    public static ApiResult<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ApiResult<T> Fail(string errorCode, string errorMessage) =>
        new() { Success = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
}
