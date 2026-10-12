using Microsoft.EntityFrameworkCore;
using SmartCart.Application.Abstractions.Security;
using SmartCart.Domain.Platform;
using SmartCart.Infrastructure.Persistence;

namespace SmartCart.Infrastructure.Security;

/// <summary>Centralized subscription / module / store-limit checks — do not scatter expiry logic in controllers.</summary>
public sealed class LicenseService : ILicenseService
{
    private readonly PlatformDbContext _db;

    public LicenseService(PlatformDbContext db)
    {
        _db = db;
    }

    public async Task AssertCanWriteAsync(Guid companyAccountId, CancellationToken cancellationToken = default)
    {
        var snap = await GetSnapshotAsync(companyAccountId, cancellationToken);
        if (!snap.CanWrite)
        {
            throw new LicenseViolationException(
                "Write access is blocked for this company. Check subscription status or renew the license.");
        }
    }

    public async Task AssertModuleAsync(
        Guid companyAccountId,
        string moduleCode,
        CancellationToken cancellationToken = default)
    {
        var snap = await GetSnapshotAsync(companyAccountId, cancellationToken);
        if (!snap.EnabledModules.Contains(moduleCode, StringComparer.OrdinalIgnoreCase))
        {
            throw new LicenseViolationException($"Module '{moduleCode}' is not enabled for this company.");
        }
    }

    public async Task AssertStoreCountAsync(
        Guid companyAccountId,
        int requestedStoreCount,
        CancellationToken cancellationToken = default)
    {
        var snap = await GetSnapshotAsync(companyAccountId, cancellationToken);
        if (requestedStoreCount > snap.MaxStoreCount)
        {
            throw new LicenseViolationException(
                $"Store limit exceeded. Allowed: {snap.MaxStoreCount}, requested: {requestedStoreCount}.");
        }
    }

    public async Task<LicenseSnapshot> GetSnapshotAsync(
        Guid companyAccountId,
        CancellationToken cancellationToken = default)
    {
        var account = await _db.CompanyAccounts
            .AsNoTracking()
            .Include(x => x.Subscription)
            .Include(x => x.Modules).ThenInclude(m => m.Module)
            .SingleOrDefaultAsync(x => x.Id == companyAccountId, cancellationToken)
            ?? throw new LicenseViolationException("Company account was not found.");

        var now = DateTimeOffset.UtcNow;
        var sub = account.Subscription;
        var expired = sub is null || sub.ExpiresOn <= now
            || string.Equals(sub.Status, SubscriptionStatuses.Expired, StringComparison.OrdinalIgnoreCase);
        var suspended = string.Equals(account.Status, CompanyAccountStatuses.Suspended, StringComparison.OrdinalIgnoreCase)
            || string.Equals(account.Status, CompanyAccountStatuses.Deactivated, StringComparison.OrdinalIgnoreCase)
            || (sub is not null && string.Equals(sub.Status, SubscriptionStatuses.Suspended, StringComparison.OrdinalIgnoreCase));

        var canWrite = string.Equals(account.Status, CompanyAccountStatuses.Ready, StringComparison.OrdinalIgnoreCase)
            && !expired
            && !suspended;

        var modules = account.Modules
            .Where(m => m.IsEnabled && m.Module.IsActive)
            .Select(m => m.Module.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new LicenseSnapshot
        {
            CompanyAccountId = account.Id,
            Status = suspended ? "Suspended" : (expired ? SubscriptionStatuses.Expired : (sub?.Status ?? "Unknown")),
            CanWrite = canWrite,
            MaxStoreCount = account.MaxStoreCount,
            StoreMode = account.StoreMode,
            ExpiresOn = sub?.ExpiresOn,
            EnabledModules = modules
        };
    }
}
