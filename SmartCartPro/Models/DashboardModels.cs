namespace SmartCartPro.Models;

#region Dashboard Home / Select

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
    public string? StoreName { get; set; }
}

public sealed class OutputDashboardQuickAction
{
    public string Title { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

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

public sealed class OutputDashboardPaymentSlice
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
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

public sealed class OutputDashboardSelect
{
    /// <summary>platform | store | welcome</summary>
    public string Mode { get; set; } = "welcome";
    public string Currency { get; set; } = "INR";
    public string TimeZoneId { get; set; } = "Asia/Kolkata";
    public string? StoreName { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool CanRefresh { get; set; }
    public List<OutputDashboardQuickAction> QuickActions { get; set; } = new();
    public OutputDashboardPlatform? Platform { get; set; }
    public OutputDashboardKpis? Kpis { get; set; }
    public List<OutputDashboardTrendPoint> Trend7 { get; set; } = new();
    public List<OutputDashboardTrendPoint> Trend30 { get; set; } = new();
    public List<OutputDashboardPaymentSlice> Payments { get; set; } = new();
    public List<OutputDashboardProductPoint> TopProducts { get; set; } = new();
    public List<OutputDashboardRecentInvoice> RecentInvoices { get; set; } = new();
    public List<OutputDashboardLowStockRow> LowStock { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

#endregion

#region Dashboard Snapshot

public sealed class InputDashboardSnapshot
{
    public int ActorUserId { get; set; }
    public int ActorFkStore { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
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
    public DateTime GeneratedAt { get; set; }
}

#endregion
