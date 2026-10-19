namespace SmartCartPro.Models;

public sealed class OutputDashboardSummary
{
    /// <summary>Number of sales completed today for the current store.</summary>
    public int SalesToday { get; set; }

    /// <summary>Count of inventory items at or below minimum stock level.</summary>
    public int LowStockCount { get; set; }

    /// <summary>Count of purchase orders in pending/draft status.</summary>
    public int PendingPurchaseOrders { get; set; }

    /// <summary>UTC timestamp of when the summary was computed.</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
