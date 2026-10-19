using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using SmartCartPro.Exceptions;
using SmartCartPro.Helpers;
using SmartCartPro.Models;

namespace SmartCartPro.Middleware;

/// <summary>
/// Global exception handler. Logs the complete error to the server console on every unhandled exception.
/// Browser Error page (non-Prod) also console.error's the developer payload. Page UI shows userMessage only.
/// </summary>
public sealed class AppExceptionHandler : IExceptionHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILogger<AppExceptionHandler> _logger;

    public AppExceptionHandler(ILogger<AppExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var mapped = ExceptionErrorMapper.Map(exception);
        var fullDeveloperMessage = ExceptionErrorMapper.BuildFullDeveloperMessage(
            mapped.DeveloperMessage,
            exception);

        // Complete error consoling — only when an error happens.
        _logger.LogError(
            exception,
            """
            Unhandled exception
            TraceId: {TraceId}
            Path: {Method} {Path}{Query}
            StatusCode: {StatusCode}
            UserMessage: {UserMessage}
            DeveloperMessage: {DeveloperMessage}
            ExceptionType: {ExceptionType}
            ExceptionMessage: {ExceptionMessage}
            StackTrace: {StackTrace}
            InnerException: {InnerException}
            """,
            context.TraceIdentifier,
            context.Request.Method,
            context.Request.Path.Value,
            context.Request.QueryString.Value,
            mapped.StatusCode,
            mapped.UserMessage,
            fullDeveloperMessage,
            exception.GetType().FullName,
            exception.Message,
            exception.StackTrace,
            exception.InnerException?.ToString() ?? "(none)");

        // Browser: friendly Error page (no developer details on screen).
        if (!ExceptionErrorMapper.WantsJsonResponse(context.Request))
        {
            return false;
        }

        IReadOnlyDictionary<string, string[]>? fieldErrors = exception is ValidationException ve
            ? ve.FieldErrors
            : null;

        var payload = CommonResponseModel.Failure(
            mapped.UserMessage,
            errors: fieldErrors is null
                ? new[] { mapped.UserMessage }
                : fieldErrors.Values.SelectMany(v => v),
            statusCode: mapped.HttpStatusCode,
            validationErrors: fieldErrors);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = mapped.HttpStatusCode;
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(payload, JsonOptions),
            cancellationToken);

        return true;
    }
}
