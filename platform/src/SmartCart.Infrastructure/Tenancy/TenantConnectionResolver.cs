using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using SmartCart.Application.Abstractions.Tenant;
using SmartCart.Domain.Platform;
using SmartCart.Infrastructure.Persistence;

namespace SmartCart.Infrastructure.Tenancy;

public sealed class TenantConnectionResolver : ITenantConnectionResolver
{
    public const string ProtectorPurpose = "SmartCart.ClientDb.ConnectionString.v1";

    private readonly PlatformDbContext _platformDb;
    private readonly IDataProtector _protector;

    public TenantConnectionResolver(PlatformDbContext platformDb, IDataProtectionProvider dataProtectionProvider)
    {
        _platformDb = platformDb;
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
    }

    public async Task<string> GetClientConnectionStringAsync(
        Guid companyAccountId,
        CancellationToken cancellationToken = default)
    {
        if (companyAccountId == Guid.Empty)
        {
            throw new InvalidOperationException("Tenant id is missing.");
        }

        var mapping = await _platformDb.ClientDatabases
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.CompanyAccountId == companyAccountId, cancellationToken);

        if (mapping is null)
        {
            throw new InvalidOperationException("Client database mapping was not found for this company.");
        }

        if (!string.Equals(mapping.ProvisioningStatus, "Ready", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Client database is not ready (status: {mapping.ProvisioningStatus}).");
        }

        // Encrypted at rest in Platform DB; never return this value to browser clients.
        return _protector.Unprotect(mapping.EncryptedConnectionString);
    }

    public string ProtectConnectionString(string connectionString) =>
        _protector.Protect(connectionString);
}
