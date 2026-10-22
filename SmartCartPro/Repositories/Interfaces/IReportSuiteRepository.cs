using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IReportSuiteRepository
{
    Task<List<OutputReportStoreOption>> AllowedStoresAsync(int userId, bool isPlatformSuperAdmin, int actorStoreId, CancellationToken ct = default);
    Task<string> CompanyNameAsync(CancellationToken ct = default);
    Task FillLookupsAsync(OutputReportScreen screen, CancellationToken ct = default);
    Task<OutputReportTable> QueryAsync(InputReportQuery input, CancellationToken ct = default);
}
