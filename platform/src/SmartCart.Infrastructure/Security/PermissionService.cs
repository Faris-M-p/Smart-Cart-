using Microsoft.EntityFrameworkCore;
using SmartCart.Application.Abstractions.Security;
using SmartCart.Application.Abstractions.Tenant;
using SmartCart.Infrastructure.Tenancy;

namespace SmartCart.Infrastructure.Security;

public sealed class PermissionService : IPermissionService
{
    private readonly ITenantContext _tenant;
    private readonly IClientDbContextFactory _clientDbFactory;

    public PermissionService(ITenantContext tenant, IClientDbContextFactory clientDbFactory)
    {
        _tenant = tenant;
        _clientDbFactory = clientDbFactory;
    }

    public async Task<bool> HasPermissionAsync(
        int employeeId,
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        if (!_tenant.IsResolved || employeeId < 1 || string.IsNullOrWhiteSpace(permissionCode))
        {
            return false;
        }

        await using var db = await _clientDbFactory.CreateAsync(_tenant.CompanyAccountId!.Value, cancellationToken);
        return await db.Employees
            .AsNoTracking()
            .Where(e => e.Id == employeeId && e.IsActive)
            .SelectMany(e => e.Role.RolePermissions)
            .AnyAsync(rp => rp.Permission.Code == permissionCode, cancellationToken);
    }

    public async Task AssertPermissionAsync(
        int employeeId,
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        if (!await HasPermissionAsync(employeeId, permissionCode, cancellationToken))
        {
            throw new ForbiddenAccessException($"Permission '{permissionCode}' is required.");
        }
    }
}
