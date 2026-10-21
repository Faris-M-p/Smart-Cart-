using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<OutputDashboardKpis> GetOperationalCountsAsync(
        List<int>? storeIds,
        CancellationToken ct = default);

    Task<List<OutputDashboardSaleFact>> ListSaleFactsAsync(
        List<int>? storeIds,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken ct = default);

    Task<List<OutputDashboardPaymentSlice>> ListPaymentsAsync(
        List<int>? storeIds,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken ct = default);

    Task<List<OutputDashboardProductPoint>> ListTopProductsAsync(
        List<int>? storeIds,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        int take,
        CancellationToken ct = default);

    Task<List<OutputDashboardRecentInvoice>> ListRecentInvoicesAsync(
        List<int>? storeIds,
        int take,
        CancellationToken ct = default);

    Task<List<OutputDashboardLowStockRow>> ListLowStockAsync(
        List<int>? storeIds,
        int take,
        CancellationToken ct = default);
}
