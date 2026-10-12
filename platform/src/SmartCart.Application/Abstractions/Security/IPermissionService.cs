namespace SmartCart.Application.Abstractions.Security;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int employeeId, string permissionCode, CancellationToken cancellationToken = default);
    Task AssertPermissionAsync(int employeeId, string permissionCode, CancellationToken cancellationToken = default);
}

public sealed class ForbiddenAccessException : InvalidOperationException
{
    public ForbiddenAccessException(string message) : base(message) { }
}
