namespace SmartCartPro.Models;

#region 1. Page Shell (Home/Index)

public sealed class InputDashboardSelect
{
    public int ActorUserId { get; set; }
    public int ActorFkStore { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
    public bool HasDashboardView { get; set; }
    public bool CanPosSell { get; set; }
    public bool CanProductsView { get; set; }
    public bool CanPurchasesManage { get; set; }
    public bool CanInventoryView { get; set; }
    public bool CanCustomersView { get; set; }
    public bool CanReportsView { get; set; }
    public bool CanReportsProfit { get; set; }
    public bool CanSettingsManage { get; set; }
    public bool CanUsersManage { get; set; }
    public bool ModulePos { get; set; }
    public bool ModuleProducts { get; set; }
    public bool ModulePurchases { get; set; }
    public bool ModuleInventory { get; set; }
    public bool ModuleCustomers { get; set; }
    public bool ModuleReports { get; set; }
    public bool ModuleSettings { get; set; }
    public bool ModuleUsers { get; set; }
    public bool FeatureLowStock { get; set; }
    public bool FeatureOnlineOrders { get; set; }
    public bool FeatureCashSession { get; set; }
    public bool FeaturePurchases { get; set; }
    public bool FeatureReturns { get; set; }
    public bool FeatureItemReport { get; set; }
    public bool FeatureCategoryReport { get; set; }
    public string? StoreName { get; set; }
}

public sealed class OutputDashboardQuickAction
{
    public string Title { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

public sealed class OutputDashboardSelect
{
    /// <summary>platform | store | welcome</summary>
    public string Mode { get; set; } = "welcome";
    public string Role { get; set; } = "owner"; // owner | manager | cashier
    public string Currency { get; set; } = "INR";
    public string TimeZoneId { get; set; } = "Asia/Kolkata";
    public string? StoreName { get; set; }
    public int StoreId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool CanRefresh { get; set; } = true;
    public bool CanSeeAdvanced { get; set; } = true;
    public bool CanSeeProfit { get; set; } = false;
    public bool ShowPosMetrics { get; set; } = true;
    public bool ShowLowStock { get; set; } = true;
    public bool ShowPurchases { get; set; } = true;
    public bool ShowOnlineOrders { get; set; } = true;
    public bool ShowCashSession { get; set; } = true;
    public List<OutputDashboardQuickAction> QuickActions { get; set; } = new();
    public OutputDashboardPlatform? Platform { get; set; }

    // Legacy fields kept for backward compatibility if needed
    public OutputDashboardKpis? Kpis { get; set; }
    public List<OutputDashboardTrendPoint> Trend7 { get; set; } = new();
    public List<OutputDashboardTrendPoint> Trend30 { get; set; } = new();
    public List<OutputDashboardPaymentSlice> Payments { get; set; } = new();
    public List<OutputDashboardProductPoint> TopProducts { get; set; } = new();
    public List<OutputDashboardRecentInvoice> RecentInvoices { get; set; } = new();
    public List<OutputDashboardLowStockRow> LowStock { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region 2. Essential View

public sealed class InputDashboardEssentials
{
    public int ActorUserId { get; set; }
    public int ActorFkStore { get; set; }
    public string? StoreName { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
    public string Range { get; set; } = "today"; // today | d7 | d30 | month
    public bool Refresh { get; set; } = false;
    public bool HasDashboardView { get; set; }
    public bool HasReportsView { get; set; }
    public bool CanPosSell { get; set; }
    public bool CanInventoryView { get; set; }
    public bool CanPurchasesManage { get; set; }
    public bool FeatureOnlineOrders { get; set; }
    public bool FeatureCashSession { get; set; }
    public bool FeatureReturns { get; set; }
    public bool FeatureLowStock { get; set; }
    public bool FeaturePurchases { get; set; }
    public bool FeatureItemReport { get; set; }
}

public sealed class OutputDashboardAlert
{
    public string Type { get; set; } = "amber"; // red | amber
    public string Icon { get; set; } = "alert";
    public string Text { get; set; } = string.Empty;
    public string LinkText { get; set; } = string.Empty;
    public string LinkUrl { get; set; } = string.Empty;
}

public sealed class OutputDashboardKpiCard
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public decimal RawValue { get; set; }
    public string FormattedValue { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public decimal? DeltaPercent { get; set; }
    public bool InvertDelta { get; set; } // true for returns where down is good
    public List<decimal>? Sparkline { get; set; }
    public string? LinkText { get; set; }
    public string? LinkUrl { get; set; }
}

public sealed class OutputDashboardSalesTrend
{
    public string Title { get; set; } = "Sales trend";
    public string Subtitle { get; set; } = string.Empty;
    public string CurrentPeriodLabel { get; set; } = "This period";
    public string PreviousPeriodLabel { get; set; } = "Previous period";
    public List<string> Labels { get; set; } = new();
    public List<decimal> CurrentValues { get; set; } = new();
    public List<decimal> PreviousValues { get; set; } = new();
}

public sealed class OutputDashboardPaymentSlice
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string FormattedAmount { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}

public sealed class OutputDashboardPaymentModes
{
    public decimal TotalCollected { get; set; }
    public string FormattedTotal { get; set; } = string.Empty;
    public string PeriodLabel { get; set; } = string.Empty;
    public List<OutputDashboardPaymentSlice> Items { get; set; } = new();
}

public sealed class OutputDashboardTopProduct
{
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public string FormattedRevenue { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string FormattedQuantity { get; set; } = string.Empty;
}

public sealed class OutputDashboardLowStockItem
{
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsOutOfStock { get; set; }
    public bool CanCreatePo { get; set; }
    public string? CreatePoUrl { get; set; }
}

public sealed class OutputDashboardRecentInvoiceItem
{
    public int IdSale { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string FormattedAmount { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = string.Empty;
    public string SaleTimeFormatted { get; set; } = string.Empty;
    public string DetailsUrl { get; set; } = string.Empty;
}

public sealed class OutputDashboardEssentials
{
    public string RangeKey { get; set; } = "today";
    public string RangeLabel { get; set; } = "Today";
    public string ComparisonLabel { get; set; } = "vs yesterday";
    public string UpdatedTime { get; set; } = string.Empty;
    public string Role { get; set; } = "owner"; // owner | manager | cashier
    public string Currency { get; set; } = "INR";
    public string StoreName { get; set; } = string.Empty;
    public List<OutputDashboardAlert> Alerts { get; set; } = new();
    public List<OutputDashboardQuickAction> QuickActions { get; set; } = new();
    public List<OutputDashboardKpiCard> Kpis { get; set; } = new();
    public OutputDashboardSalesTrend SalesTrend { get; set; } = new();
    public OutputDashboardPaymentModes Payments { get; set; } = new();
    public List<OutputDashboardTopProduct> TopProducts { get; set; } = new();
    public string? ItemSalesReportUrl { get; set; }
    public List<OutputDashboardLowStockItem> LowStock { get; set; } = new();
    public string? AllLowStockUrl { get; set; }
    public List<OutputDashboardRecentInvoiceItem> RecentInvoices { get; set; } = new();
    public string? AllInvoicesUrl { get; set; }
}

#endregion

#region 3. Advanced Insights (Lazy)

public sealed class InputDashboardAdvanced
{
    public int ActorUserId { get; set; }
    public int ActorFkStore { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
    public string Range { get; set; } = "today";
    public bool Refresh { get; set; } = false;
    public bool CanSeeProfit { get; set; }
    public bool FeatureOnlineOrders { get; set; }
    public bool FeaturePurchases { get; set; }
    public bool FeatureCategoryReport { get; set; }
}

// 3.1 Advanced Sales: Hourly, DayOfWeek, CounterVsOnline, Top 10 (Revenue + Qty)
public sealed class OutputDashboardHourlyPoint
{
    public int Hour { get; set; } // 8 to 22
    public string Label { get; set; } = string.Empty; // e.g. "9a", "2p"
    public decimal Sales { get; set; }
}

public sealed class OutputDashboardDayOfWeekPoint
{
    public string DayName { get; set; } = string.Empty; // Mon, Tue, ...
    public decimal Sales { get; set; }
}

public sealed class OutputDashboardChannelItem
{
    public string Channel { get; set; } = string.Empty; // Counter (POS), Online
    public decimal Sales { get; set; }
    public string FormattedSales { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public string Color { get; set; } = string.Empty;
}

public sealed class OutputDashboardAdvancedSales
{
    public List<string> HourlyLabels { get; set; } = new();
    public List<decimal> HourlyValues { get; set; } = new();
    public List<string> DayOfWeekLabels { get; set; } = new();
    public List<decimal> DayOfWeekValues { get; set; } = new();
    public List<OutputDashboardChannelItem> Channels { get; set; } = new();
    public decimal TotalSales { get; set; }
    public string FormattedTotalSales { get; set; } = string.Empty;
    public List<OutputDashboardTopProduct> Top10Revenue { get; set; } = new();
    public List<OutputDashboardTopProduct> Top10Quantity { get; set; } = new();
}

// 3.2 Advanced Categories: Top 8 + Others
public sealed class OutputDashboardCategoryItem
{
    public int FkCategory { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Sales { get; set; }
    public string FormattedSales { get; set; } = string.Empty;
    public decimal SharePercentage { get; set; }
    public string Color { get; set; } = string.Empty;
}

public sealed class OutputDashboardAdvancedCategories
{
    public List<OutputDashboardCategoryItem> Categories { get; set; } = new();
    public string? CategoryReportUrl { get; set; }
}

// 3.3 Advanced Stock: Stock Health Donut
public sealed class OutputDashboardStockHealthItem
{
    public string Status { get; set; } = string.Empty; // In stock, Low, Out of stock, Non-moving 30d
    public int Count { get; set; }
    public string Color { get; set; } = string.Empty;
}

public sealed class OutputDashboardAdvancedStock
{
    public int TotalTracked { get; set; }
    public List<OutputDashboardStockHealthItem> HealthItems { get; set; } = new();
}

// 3.4 Advanced Purchases: Sales vs Purchases Last 6 Months
public sealed class OutputDashboardAdvancedPurchases
{
    public List<string> MonthLabels { get; set; } = new();
    public List<decimal> SalesValues { get; set; } = new();
    public List<decimal> PurchaseValues { get; set; } = new();
}

// 3.5 Advanced Team: Cashier Performance & Store Comparison
public sealed class OutputDashboardCashierPerformance
{
    public int UserId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public int BillsCount { get; set; }
    public decimal SalesTotal { get; set; }
    public string FormattedSales { get; set; } = string.Empty;
    public decimal AvgBillValue { get; set; }
    public string FormattedAvgBill { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
}

public sealed class OutputDashboardStoreComparisonItem
{
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public decimal SalesTotal { get; set; }
    public string FormattedSales { get; set; } = string.Empty;
}

public sealed class OutputDashboardAdvancedTeam
{
    public List<OutputDashboardCashierPerformance> Cashiers { get; set; } = new();
    public List<OutputDashboardStoreComparisonItem> StoreComparison { get; set; } = new();
}

// 3.6 Advanced Profit: Gross profit, margin %, margin by category
public sealed class OutputDashboardCategoryMargin
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal MarginPercent { get; set; }
}

public sealed class OutputDashboardAdvancedProfit
{
    public decimal GrossProfit { get; set; }
    public string FormattedGrossProfit { get; set; } = string.Empty;
    public decimal MarginPercent { get; set; }
    public decimal? DeltaMarginPercent { get; set; }
    public List<OutputDashboardCategoryMargin> CategoryMargins { get; set; } = new();
}

#endregion

#region 4. Legacy Support (Backward Compatibility)

public sealed class OutputDashboardKpis
{
    public decimal SalesToday { get; set; }
    public decimal SalesYesterday { get; set; }
    public decimal TodayChangePercent { get; set; }
    public decimal SalesMonth { get; set; }
    public int InvoiceCountToday { get; set; }
    public int LowStockCount { get; set; }
    public int PendingPurchaseOrders { get; set; }
    public int PendingOnlineOrders { get; set; }
    public int OpenCashSessions { get; set; }
}

public sealed class OutputDashboardSaleFact
{
    public DateTime SaleDate { get; set; }
    public decimal TotalAmount { get; set; }
}

public sealed class OutputDashboardTrendPoint
{
    public DateOnly Date { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal NetSales { get; set; }
    public int InvoiceCount { get; set; }
}

public sealed class OutputDashboardProductPoint
{
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class OutputDashboardRecentInvoice
{
    public int IdSale { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime SaleDate { get; set; }
}

public sealed class OutputDashboardLowStockRow
{
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal ReorderLevel { get; set; }
}

public sealed class OutputDashboardPlatform
{
    public string LicenseType { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public DateOnly ExpiryDate { get; set; }
    public int StoresUsed { get; set; }
    public int MaxStores { get; set; }
    public int ModuleCount { get; set; }
}

public sealed class InputDashboardSnapshot
{
    public int ActorUserId { get; set; }
    public int ActorFkStore { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
    public bool ModulePos { get; set; }
    public bool FeatureLowStock { get; set; }
    public bool FeatureOnlineOrders { get; set; }
    public bool FeatureCashSession { get; set; }
    public bool FeaturePurchases { get; set; }
}

public sealed class OutputDashboardSnapshot
{
    public string Currency { get; set; } = "INR";
    public string TimeZoneId { get; set; } = "Asia/Kolkata";
    public OutputDashboardKpis Kpis { get; set; } = new();
    public List<OutputDashboardTrendPoint> Trend7 { get; set; } = new();
    public List<OutputDashboardTrendPoint> Trend30 { get; set; } = new();
    public List<OutputDashboardPaymentSlice> Payments { get; set; } = new();
    public List<OutputDashboardProductPoint> TopProducts { get; set; } = new();
    public List<OutputDashboardRecentInvoice> RecentInvoices { get; set; } = new();
    public List<OutputDashboardLowStockRow> LowStock { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

#endregion
