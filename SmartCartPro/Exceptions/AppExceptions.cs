namespace SmartCartPro.Exceptions;

/// <summary>
/// Base for domain/API exceptions. Every typed exception carries
/// <see cref="UserMessage"/> (safe for clients) and <see cref="DeveloperMessage"/> (diagnostics).
/// The global handler only returns the full developer message outside Prod.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string userMessage, string? developerMessage = null, Exception? inner = null)
        : base(developerMessage ?? userMessage, inner)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            throw new ArgumentException("User message is required.", nameof(userMessage));
        }

        UserMessage = userMessage.Trim();
        DeveloperMessage = string.IsNullOrWhiteSpace(developerMessage)
            ? UserMessage
            : developerMessage.Trim();
    }

    /// <summary>Safe message for UI / API clients.</summary>
    public string UserMessage { get; }

    /// <summary>Technical detail for developers; visibility gated by environment in the handler.</summary>
    public string DeveloperMessage { get; }
}

public sealed class ResourceNotFoundException : AppException
{
    public ResourceNotFoundException(string userMessage, string? developerMessage = null)
        : base(userMessage, developerMessage)
    {
    }
}

public sealed class NoDataFoundException : AppException
{
    public NoDataFoundException(string userMessage, string? developerMessage = null)
        : base(userMessage, developerMessage)
    {
    }
}

public sealed class ValidationException : AppException
{
    public ValidationException(string userMessage, string? developerMessage = null)
        : base(userMessage, developerMessage)
    {
    }
}

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Authentication failed" : userMessage,
            developerMessage)
    {
    }
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Access denied" : userMessage,
            developerMessage)
    {
    }
}

public sealed class MethodNotAllowedException : AppException
{
    public MethodNotAllowedException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Method not allowed" : userMessage,
            developerMessage)
    {
    }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Resource conflict" : userMessage,
            developerMessage)
    {
    }
}

public sealed class UnsupportedMediaTypeException : AppException
{
    public UnsupportedMediaTypeException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Unsupported media type" : userMessage,
            developerMessage)
    {
    }
}

public sealed class ServiceUnavailableException : AppException
{
    public ServiceUnavailableException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Service temporarily unavailable" : userMessage,
            developerMessage)
    {
    }
}

public sealed class BadGatewayException : AppException
{
    public BadGatewayException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Something went wrong. Please try again later." : userMessage,
            developerMessage)
    {
    }
}

public sealed class PreconditionFailedException : AppException
{
    public PreconditionFailedException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Precondition failed" : userMessage,
            developerMessage)
    {
    }
}

public sealed class TooManyRequestsException : AppException
{
    public TooManyRequestsException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Too many requests" : userMessage,
            developerMessage)
    {
    }
}

public sealed class DecryptionFailedException : AppException
{
    public DecryptionFailedException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Data processing failed" : userMessage,
            developerMessage)
    {
    }
}

public sealed class ArgumentExceptions : AppException
{
    public ArgumentExceptions(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Invalid argument" : userMessage,
            developerMessage)
    {
    }
}

public sealed class UpdateFailedException : AppException
{
    public UpdateFailedException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Update operation failed" : userMessage,
            developerMessage)
    {
    }
}

public sealed class SessionExpiredException : AppException
{
    public SessionExpiredException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Session expired, please login again" : userMessage,
            developerMessage)
    {
    }
}

public sealed class AccessTokenExpiredException : AppException
{
    public AccessTokenExpiredException(string userMessage, string? developerMessage = null)
        : base(
            string.IsNullOrWhiteSpace(userMessage) ? "Access token expired, please refresh token" : userMessage,
            developerMessage)
    {
    }
}
