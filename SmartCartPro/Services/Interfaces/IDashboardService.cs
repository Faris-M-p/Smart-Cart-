using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IDashboardService
{
    Task<OutputDashboardSelect> GetHomeAsync(InputDashboardSelect input, CancellationToken ct = default);
    Task<OutputDashboardEssentials> GetEssentialsAsync(InputDashboardEssentials input, CancellationToken ct = default);
    Task<OutputDashboardSnapshot> GetSnapshotAsync(InputDashboardSnapshot input, CancellationToken ct = default);

    Task<OutputDashboardAdvancedSales> GetAdvancedSalesAsync(InputDashboardAdvanced input, CancellationToken ct = default);
    Task<OutputDashboardAdvancedCategories> GetAdvancedCategoriesAsync(InputDashboardAdvanced input, CancellationToken ct = default);
    Task<OutputDashboardAdvancedStock> GetAdvancedStockAsync(InputDashboardAdvanced input, CancellationToken ct = default);
    Task<OutputDashboardAdvancedPurchases> GetAdvancedPurchasesAsync(InputDashboardAdvanced input, CancellationToken ct = default);
    Task<OutputDashboardAdvancedTeam> GetAdvancedTeamAsync(InputDashboardAdvanced input, CancellationToken ct = default);
    Task<OutputDashboardAdvancedProfit> GetAdvancedProfitAsync(InputDashboardAdvanced input, CancellationToken ct = default);
}
