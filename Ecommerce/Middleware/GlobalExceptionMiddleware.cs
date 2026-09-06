using System.Text.Json;

namespace Ecommerce.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for request {Method} {Path}", context.Request.Method, context.Request.Path);
            if (context.Response.HasStarted)
            {
                throw;
            }

            await HandleExceptionAsync(context, ex, _environment.IsDevelopment());
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex, bool includeDetails)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        object response;
         if (includeDetails)
        {
            response = new
            {
                success = false,
                exceptionType = ex.GetType().FullName,
                message = ex.Message,
                innerMessage = ex.InnerException?.Message,
                path = context.Request.Path.Value,
                method = context.Request.Method,
                traceId = context.TraceIdentifier
            };
        }
        else
        {
            response = new
            {
                success = false,
                message = "Something went wrong. Please try again later.",
                traceId = context.TraceIdentifier
            };
        }

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
