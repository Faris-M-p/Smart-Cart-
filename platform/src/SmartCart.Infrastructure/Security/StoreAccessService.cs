using Microsoft.EntityFrameworkCore;
using SmartCart.Application.Abstractions.Security;
using SmartCart.Application.Abstractions.Tenant;
using SmartCart.Domain.Platform;
using SmartCart.Infrastructure.Persistence;
using SmartCart.Infrastructure.Tenancy;

namespace SmartCart.Infrastructure.Security;

public sealed class StoreAccessService : IStoreAccessService
{
    private readonly ITenantContext _tenant;
    private readonly PlatformDbContext _platformDb;
    private readonly IClientDbContextFactory _clientDbFactory;

    public StoreAccessService(
        ITenantContext tenant,
        PlatformDbContext platformDb,
        IClientDbContextFactory clientDbFactory)
    {
        _tenant = tenant;
        _platformDb = platformDb;
        _clientDbFactory = clientDbFactory;
    }

    public async Task<IReadOnlyList<int>> GetAuthorizedStoreIdsAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();
        await using var db = await _clientDbFactory.CreateAsync(_tenant.CompanyAccountId!.Value, cancellationToken);
        return await db.EmployeeStoreAccess
            .AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .Select(x => x.StoreId)
            .ToListAsync(cancellationToken);
    }

    public async Task AssertStoreAccessAsync(
        int employeeId,
        int storeId,
        CancellationToken cancellationToken = default)
    {
        var ids = await GetAuthorizedStoreIdsAsync(employeeId, cancellationToken);
        if (!ids.Contains(storeId))
        {
            throw new ForbiddenAccessException("You do not have access to this store.");
        }
    }

    public async Task<int> ResolveStoreIdAsync(
        int employeeId,
        int? requestedStoreId,
        CancellationToken cancellationToken = default)
    {
        EnsureTenant();

        var account = await _platformDb.CompanyAccounts.AsNoTracking()
            .SingleAsync(x => x.Id == _tenant.CompanyAccountId, cancellationToken);

        await using var db = await _clientDbFactory.CreateAsync(_tenant.CompanyAccountId!.Value, cancellationToken);

        if (string.Equals(account.StoreMode, StoreModes.Single, StringComparison.OrdinalIgnoreCase))
        {
            var defaultStore = await db.Stores.AsNoTracking()
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.IsDefault)
                .ThenBy(s => s.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new ForbiddenAccessException("No active store is configured for this company.");

            return defaultStore.Id;
        }

        if (!requestedStoreId.HasValue || requestedStoreId.Value < 1)
        {
            throw new ForbiddenAccessException("StoreId is required in multi-store mode.");
        }

        await AssertStoreAccessAsync(employeeId, requestedStoreId.Value, cancellationToken);
        return requestedStoreId.Value;
    }

    private void EnsureTenant()
    {
        if (!_tenant.IsResolved)
        {
            throw new ForbiddenAccessException("Tenant context is not resolved.");
        }
    }
}
