using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using SmartCartPro.Configuration;
using SmartCartPro.Helpers;

namespace SmartCartPro.Middleware;

/// <summary>
/// Global exception handler. API/JSON requests get a JSON envelope.
/// Browser (HTML) requests return false so <c>/Home/Error</c> renders a friendly page.
/// Full developerMessage is only exposed when EnvironmentSettings is not Prod.
/// </summary>
public sealed class AppExceptionHandler : IExceptionHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ILogger<AppExceptionHandler> _logger;
    private readonly EnvironmentSettings _environmentSettings;

    public AppExceptionHandler(
        ILogger<AppExceptionHandler> logger,
        IOptions<EnvironmentSettings> environmentSettings)
    {
        _logger = logger;
        _environmentSettings = environmentSettings?.Value
            ?? throw new ArgumentNullException(nameof(environmentSettings));
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception has occurred.");

        // Let MVC re-execute /Home/Error for browser navigations (friendly page).
        if (!ExceptionErrorMapper.WantsJsonResponse(context.Request))
        {
            return false;
        }

        var mapped = ExceptionErrorMapper.Map(exception);
        var showFullDeveloperMessage = !_environmentSettings.IsProduction;
        var exposedDeveloperMessage = showFullDeveloperMessage
            ? ExceptionErrorMapper.BuildFullDeveloperMessage(mapped.DeveloperMessage, exception)
            : string.Empty;

        var payload = new
        {
            statusCode = mapped.StatusCode,
            status = "Error",
            statusMessage = mapped.UserMessage,
            userMessage = mapped.UserMessage,
            developerMessage = exposedDeveloperMessage,
            error = new
            {
                errorCode = mapped.StatusCode.ToString(),
                message = mapped.UserMessage,
                userMessage = mapped.UserMessage,
                developerMessage = exposedDeveloperMessage
            },
            data = string.Empty,
            timestamp = DateTime.UtcNow.ToString("o")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = mapped.HttpStatusCode;
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(payload, JsonOptions),
            cancellationToken);

        return true;
    }
}
