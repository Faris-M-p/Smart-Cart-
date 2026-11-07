using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IDashboardRepository
{
    // Essential Queries
    Task<(decimal Sales, int Bills)> GetSalesSummaryAsync(int storeId, int? userId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken ct = default);
    Task<(decimal Returns, int ReturnCount)> GetReturnsSummaryAsync(int storeId, int? userId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken ct = default);
    Task<List<OutputDashboardPaymentSlice>> ListPaymentsAsync(int storeId, int? userId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken ct = default);
    Task<List<OutputDashboardSaleFact>> ListSaleFactsAsync(int storeId, int? userId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken ct = default);
    Task<List<OutputDashboardSaleFact>> ListSaleFactsAsync(List<int>? storeIds, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken ct = default);
    Task<List<OutputDashboardTopProduct>> ListTopProductsAsync(int storeId, int? userId, DateTime fromUtc, DateTime toUtcExclusive, int take, CancellationToken ct = default);
    Task<List<OutputDashboardLowStockItem>> ListLowStockAsync(int storeId, int take, CancellationToken ct = default);
    Task<List<OutputDashboardRecentInvoiceItem>> ListRecentInvoicesAsync(int storeId, int? userId, int take, CancellationToken ct = default);
    Task<(int LowStockCount, int OutOfStockCount, int PendingPos, int PendingOnlineOrders, int OpenCashSessions, string? OpenCashierName, decimal OpenExpectedCash)> GetOperationalStatusAsync(int storeId, int? userId, CancellationToken ct = default);
    Task<(decimal? LastVariance, string? LastCashierName)> GetLastClosedSessionDifferenceAsync(int storeId, CancellationToken ct = default);
    Task<(int PendingCount, DateTime? OldestOrderDate)> GetOldestPendingOnlineOrderAsync(int storeId, CancellationToken ct = default);
    Task<DateTime?> GetCashierOpenSessionTimeAsync(int storeId, int userId, CancellationToken ct = default);
    Task<DateOnly?> GetLicenseExpiryDateAsync(DateOnly today, CancellationToken ct = default);

    // Advanced Queries
    Task<List<OutputDashboardHourlyPoint>> ListHourlySalesAsync(int storeId, DateTime fromUtc, DateTime toUtcExclusive, TimeZoneInfo tz, CancellationToken ct = default);
    Task<List<OutputDashboardDayOfWeekPoint>> ListDayOfWeekSalesAsync(int storeId, DateTime fromUtc, DateTime toUtcExclusive, TimeZoneInfo tz, CancellationToken ct = default);
    Task<(decimal CounterSales, decimal OnlineSales)> GetCounterVsOnlineSalesAsync(int storeId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken ct = default);
    Task<List<OutputDashboardTopProduct>> ListTopProductsByQuantityAsync(int storeId, DateTime fromUtc, DateTime toUtcExclusive, int take, CancellationToken ct = default);
    Task<List<OutputDashboardCategoryItem>> ListCategorySalesAsync(int storeId, DateTime fromUtc, DateTime toUtcExclusive, int take, CancellationToken ct = default);
    Task<(int InStock, int Low, int Out, int NonMoving30d, int Total)> GetStockHealthAsync(int storeId, CancellationToken ct = default);
    Task<List<(int Year, int Month, decimal Sales, decimal Purchases)>> ListSalesVsPurchases6MonthsAsync(int storeId, DateOnly currentMonthStart, CancellationToken ct = default);
    Task<List<OutputDashboardCashierPerformance>> ListCashierPerformanceAsync(int storeId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken ct = default);
    Task<List<OutputDashboardStoreComparisonItem>> ListStoreComparisonAsync(List<int> accessibleStoreIds, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken ct = default);
    Task<List<int>> GetAccessibleStoreIdsAsync(int userId, bool isPlatformSuperAdmin, CancellationToken ct = default);
    Task<(decimal GrossProfit, decimal NetSales, decimal MarginPercent)> GetGrossProfitAsync(int storeId, DateTime fromUtc, DateTime toUtcExclusive, CancellationToken ct = default);
    Task<List<OutputDashboardCategoryMargin>> ListCategoryMarginsAsync(int storeId, DateTime fromUtc, DateTime toUtcExclusive, int take, CancellationToken ct = default);

    // Legacy Support
    Task<OutputDashboardKpis> GetOperationalCountsAsync(List<int>? storeIds, CancellationToken ct = default);
}
