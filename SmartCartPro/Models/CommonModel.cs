namespace SmartCartPro.Models;

/// <summary>Shared enums and AJAX response envelope for SmartCartPro.</summary>
public class CommonModel
{
    /// <summary>
    /// License status (int for view/input). DB column <c>license.status</c> stores TEXT:
    /// active | suspended | expired | trial.
    /// </summary>
    public enum LicenseStatus
    {
        Active = 1,
        Suspended = 2,
        Expired = 3,
        Trial = 4
    }
}

/// <summary>
/// Shared AJAX envelope. Distinguish success vs failure via <see cref="StatusCode"/> / <see cref="Status"/>.
/// </summary>
public class CommonResponseModel
{
    public int StatusCode { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public object? Data { get; set; }

    public List<string>? Errors { get; set; }

    public string Timestamp { get; set; } = string.Empty;

    public static CommonResponseModel Success(string message, object? data = null) => new()
    {
        StatusCode = 200,
        Status = "success",
        Message = message,
        Data = data,
        Errors = null,
        Timestamp = DateTime.UtcNow.ToString("o")
    };

    public static CommonResponseModel Failure(
        string message,
        IEnumerable<string>? errors = null,
        int statusCode = 400) => new()
    {
        StatusCode = statusCode,
        Status = "error",
        Message = message,
        Data = null,
        Errors = errors?.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().ToList()
                 ?? new List<string>(),
        Timestamp = DateTime.UtcNow.ToString("o")
    };
}
