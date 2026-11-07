using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface ICompanyService
{
    Task<OutputCompanySelect> GetCompanyAsync(InputCompanySelect input, CancellationToken ct = default);
    Task<OutputCompanyCreate> CreateCompanyAsync(InputCompanyCreate input, CancellationToken ct = default);
    Task<OutputCompanyUpdate> UpdateCompanyAsync(InputCompanyUpdate input, CancellationToken ct = default);

    Task<OutputStoreList> ListStoresAsync(InputStoreList input, CancellationToken ct = default);
    Task<OutputStoreSelect?> GetStoreAsync(InputStoreSelect input, CancellationToken ct = default);
    Task<OutputStoreAccessibleList> ListAccessibleStoresAsync(InputStoreAccessibleList input, CancellationToken ct = default);
    Task<OutputStoreCreate> CreateStoreAsync(InputStoreCreate input, CancellationToken ct = default);
    Task<OutputStoreUpdate> UpdateStoreAsync(InputStoreUpdate input, CancellationToken ct = default);
    Task<OutputStoreStatusUpdate> SetStoreActiveAsync(InputStoreStatusUpdate input, CancellationToken ct = default);

    Task<OutputLicenseSelect?> GetLicenseAsync(InputLicenseSelect input, CancellationToken ct = default);
    Task<DateOnly> GetLicenseTodayAsync(CancellationToken ct = default);
    Task<int> GetActiveStoreCountForLicenseAsync(CancellationToken ct = default);
    Task<int> GetActiveUserCountForLicenseAsync(CancellationToken ct = default);
    Task<List<OutputLicenseSelect>> ListLicensesAsync(CancellationToken ct = default);
    Task<OutputLicenseSelect?> GetLicenseByIdAsync(int idLicense, CancellationToken ct = default);
    Task<OutputLicenseUpdate> CreateLicenseAsync(InputLicenseCreate input, CancellationToken ct = default);
    Task<OutputLicenseUpdate> UpdateLicenseAsync(InputLicenseUpdate input, CancellationToken ct = default);
    Task<OutputLicenseStatusUpdate> SetLicenseStatusAsync(InputLicenseStatusUpdate input, CancellationToken ct = default);
}
