using System.Collections.Generic;

namespace SmartCartPro.Models;

/// <summary>
/// Shared AJAX envelope for Catalog create/update/delete (trial).
/// Success and failure use the same shape; distinguish via <see cref="StatusCode"/> / <see cref="Status"/>.
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
