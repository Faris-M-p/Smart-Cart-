using SmartCartPro.Exceptions;

namespace SmartCartPro.Helpers;

/// <summary>Maps exceptions to status codes + user/developer messages for the global handler and error pages.</summary>
public static class ExceptionErrorMapper
{
    public sealed record ErrorInfo(
        int StatusCode,
        int HttpStatusCode,
        string UserMessage,
        string DeveloperMessage);

    public static ErrorInfo Map(Exception exception)
    {
        var statusCode = StatusCodes.Status500InternalServerError;
        var userMessage = "Something went wrong. Please try again later.";
        var developerMessage = exception.Message;

        switch (exception)
        {
            case AppException appException:
                userMessage = appException.UserMessage;
                developerMessage = appException.DeveloperMessage;
                statusCode = MapAppExceptionStatusCode(appException);
                if (appException is ValidationException validationException)
                {
                    var parts = validationException.UserMessage.Split('/', 2);
                    if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                    {
                        userMessage = parts[0].Trim();
                    }
                }
                break;

            case TimeoutException:
                statusCode = StatusCodes.Status408RequestTimeout;
                userMessage = "Request timeout";
                break;

            case NotImplementedException:
                statusCode = StatusCodes.Status501NotImplemented;
                userMessage = "Feature not implemented";
                break;

            case ArgumentException:
                statusCode = StatusCodes.Status400BadRequest;
                userMessage = "Invalid argument";
                break;
        }

        var httpStatusCode = exception switch
        {
            SessionExpiredException or AccessTokenExpiredException => StatusCodes.Status401Unauthorized,
            _ => statusCode
        };

        return new ErrorInfo(statusCode, httpStatusCode, userMessage, developerMessage);
    }

    public static string BuildFullDeveloperMessage(string developerMessage, Exception exception)
    {
        var detail = string.IsNullOrWhiteSpace(developerMessage)
            ? exception.Message
            : developerMessage;

        var innerMessages = new List<string>();
        var curr = exception.InnerException;
        while (curr != null)
        {
            innerMessages.Add(curr.Message);
            curr = curr.InnerException;
        }

        var innerText = innerMessages.Count > 0
            ? "\nInner Exceptions: " + string.Join(" --> ", innerMessages)
            : string.Empty;

        return $"{detail}{innerText}\nStack Trace:\n{exception.StackTrace}";
    }

    public static bool WantsJsonResponse(HttpRequest request)
    {
        if (string.Equals(
                request.Headers["X-Requested-With"],
                "XMLHttpRequest",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // POST/PUT/DELETE/PATCH on these MVC controller prefixes always return JSON
        // so that AppExceptionHandler serialises thrown exceptions as CommonResponseModel.
        if (IsMutatingMethod(request.Method)
            && IsMvcJsonControllerPath(request.Path))
        {
            return true;
        }

        var accept = request.Headers.Accept.ToString();
        if (string.IsNullOrWhiteSpace(accept))
        {
            return false;
        }

        return accept.Contains("application/json", StringComparison.OrdinalIgnoreCase)
               && !accept.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMutatingMethod(string method) =>
        HttpMethods.IsPost(method)
        || HttpMethods.IsPut(method)
        || HttpMethods.IsDelete(method)
        || HttpMethods.IsPatch(method);

    private static readonly string[] JsonControllerPrefixes =
    [
        "/api",
        "/Filter",
        "/Catalog",
        "/Company",
        "/Store",
        "/License",
        "/Module",
        "/Employee",
        "/Role",
        "/Product",
        "/Customers",
        "/Inventory",
        "/Supplier",
        "/Purchase",
        "/Sales",
        "/Setting",
        "/ProductReview",
        "/OnlineOrder",
        "/Cart"
    ];

    private static bool IsMvcJsonControllerPath(PathString path) =>
        JsonControllerPrefixes.Any(prefix =>
            path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase));

    private static int MapAppExceptionStatusCode(AppException exception) =>
        exception switch
        {
            ResourceNotFoundException or NoDataFoundException => StatusCodes.Status404NotFound,
            ValidationException or ArgumentExceptions => StatusCodes.Status400BadRequest,
            UnauthorizedException => StatusCodes.Status401Unauthorized,
            ForbiddenException => StatusCodes.Status403Forbidden,
            MethodNotAllowedException => StatusCodes.Status405MethodNotAllowed,
            ConflictException => StatusCodes.Status409Conflict,
            PreconditionFailedException => StatusCodes.Status412PreconditionFailed,
            UnsupportedMediaTypeException => StatusCodes.Status415UnsupportedMediaType,
            DecryptionFailedException or UpdateFailedException => StatusCodes.Status422UnprocessableEntity,
            TooManyRequestsException => StatusCodes.Status429TooManyRequests,
            BadGatewayException => StatusCodes.Status502BadGateway,
            ServiceUnavailableException => StatusCodes.Status503ServiceUnavailable,
            SessionExpiredException => 440,
            AccessTokenExpiredException => 498,
            _ => StatusCodes.Status500InternalServerError
        };
}
