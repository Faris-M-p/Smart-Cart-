namespace SmartCart.Application.Abstractions.Tenant;

public interface ITenantConnectionResolver
{
    /// <summary>
    /// Resolves the client database connection for a platform company account.
    /// Must only be called with a trusted CompanyAccountId from auth claims / Super Admin selection.
    /// </summary>
    Task<string> GetClientConnectionStringAsync(Guid companyAccountId, CancellationToken cancellationToken = default);
}
