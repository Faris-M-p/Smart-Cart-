using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IDashboardService
{
    Task<OutputDashboardSelect> GetHomeAsync(InputDashboardSelect input, CancellationToken ct = default);
    Task<OutputDashboardSnapshot> GetSnapshotAsync(InputDashboardSnapshot input, CancellationToken ct = default);
}
