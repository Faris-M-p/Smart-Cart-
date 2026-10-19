using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface ICompanyRepository
{
    Task<OutputCompanySelect?> GetCompanyAsync(InputCompanySelect input, CancellationToken ct = default);
    Task<OutputCompanyCreate> CreateCompanyAsync(InputCompanyCreate input, CancellationToken ct = default);
    Task<OutputCompanyUpdate> UpdateCompanyAsync(InputCompanyUpdate input, CancellationToken ct = default);

    Task<OutputStoreSelect?> GetStoreAsync(InputStoreSelect input, CancellationToken ct = default);
    Task<OutputStoreList> ListStoresAsync(InputStoreList input, CancellationToken ct = default);
    Task<int> GetActiveStoreCountAsync(CancellationToken ct = default);
    Task<OutputStoreCreate> CreateStoreAsync(InputStoreCreate input, CancellationToken ct = default);
    Task<OutputStoreUpdate> UpdateStoreAsync(InputStoreUpdate input, CancellationToken ct = default);

    Task<OutputLicenseSelect?> GetLicenseAsync(InputLicenseSelect input, CancellationToken ct = default);
    Task<OutputLicenseUpdate> UpdateLicenseAsync(InputLicenseUpdate input, CancellationToken ct = default);

    Task AddAuditLogAsync(InputAuditLogCreate input, CancellationToken ct = default);
}
