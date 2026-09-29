namespace Ecommerce.Models.Admin
{
    public class DashboardModel
    {
        public class DashboardSummary
        {
            public DashboardKpis Kpis { get; set; } = new();
            public List<DashboardNamedCount> OrderStatusCounts { get; set; } = new();
            public List<DashboardDailyPoint> OrderTrend { get; set; } = new();
            public List<DashboardDailyPoint> PosSalesTrend { get; set; } = new();
            public List<DashboardNamedAmount> TopProducts { get; set; } = new();
            public List<DashboardLowStockItem> LowStockItems { get; set; } = new();
            public List<DashboardRecentOrder> RecentOrders { get; set; } = new();
            public DateTime GeneratedAt { get; set; }
        }

        public class DashboardKpis
        {
            public int TotalOrders { get; set; }
            public int OrdersToday { get; set; }
            public int PendingOrders { get; set; }
            public decimal OnlineRevenue { get; set; }
            public decimal OnlineRevenueToday { get; set; }
            public decimal PosRevenue { get; set; }
            public int ActiveProducts { get; set; }
            public int ActiveSkus { get; set; }
            public int Customers { get; set; }
            public int LowStockSkuCount { get; set; }
            public int SellOnlineSkuCount { get; set; }
        }

        public class DashboardNamedCount
        {
            public string Name { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        public class DashboardDailyPoint
        {
            public string Date { get; set; } = string.Empty;
            public int Count { get; set; }
            public decimal Amount { get; set; }
        }

        public class DashboardNamedAmount
        {
            public string Name { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal Amount { get; set; }
        }

        public class DashboardLowStockItem
        {
            public int ProductVariantId { get; set; }
            public string Sku { get; set; } = string.Empty;
            public string ProductName { get; set; } = string.Empty;
            public int AvailableQty { get; set; }
            public int ReorderLevel { get; set; }
        }

        public class DashboardRecentOrder
        {
            public int OrderId { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public decimal TotalAmount { get; set; }
            public DateTime? OrderDate { get; set; }
            public bool Cancelled { get; set; }
        }
    }
}
