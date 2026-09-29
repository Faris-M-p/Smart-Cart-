using Ecommerce.DataAccess;
using Ecommerce.Interface.Admin;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.DashboardModel;

namespace Ecommerce.Repository.Admin
{
    public class DashboardRepository : IDashboardInterface
    {
        private const int LowStockThreshold = 10;
        private const int TrendDays = 14;
        private readonly EcommerceDbContext _db;

        public DashboardRepository(EcommerceDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.Today;
            var trendFrom = today.AddDays(-(TrendDays - 1));

            var orders = _db.Orders.AsNoTracking();
            var activeOrders = orders.Where(o => o.Cancelled != true);
            var products = _db.Products.AsNoTracking().Where(p => !p.Cancelled);
            var variants = _db.ProductVariants.AsNoTracking().Where(v => !v.Cancelled);
            var users = _db.Users.AsNoTracking().Where(u => u.Cancelled != true);
            var sales = _db.Sales.AsNoTracking().Where(s => !s.Cancelled);

            var totalOrders = await activeOrders.CountAsync(cancellationToken);
            var ordersToday = await activeOrders.CountAsync(
                o => o.OrderDate != null && o.OrderDate.Value.Date == today,
                cancellationToken);
            var pendingOrders = await activeOrders.CountAsync(
                o => o.OrderStatus == "Placed"
                     || o.OrderStatus == "Pending"
                     || o.OrderStatus == "Confirmed",
                cancellationToken);
            var onlineRevenue = await activeOrders.SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;
            var onlineRevenueToday = await activeOrders
                .Where(o => o.OrderDate != null && o.OrderDate.Value.Date == today)
                .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;
            var posRevenue = await sales.SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;
            var activeProducts = await products.CountAsync(p => p.IsActive, cancellationToken);
            var activeSkus = await variants.CountAsync(v => v.IsActive, cancellationToken);
            var sellOnlineSkuCount = await variants.CountAsync(v => v.IsActive && v.SellOnline, cancellationToken);
            var customers = await users.CountAsync(cancellationToken);

            var stockBySku = await (
                from pv in variants
                join p in products on pv.FK_Product equals p.ID_Product
                join s in _db.Stock.AsNoTracking() on pv.ID_ProductVariant equals s.FK_ProductVariant into sg
                select new
                {
                    pv.ID_ProductVariant,
                    pv.SKU,
                    ProductName = p.Name,
                    AvailableQty = sg.Where(x => !x.Cancelled).Sum(x => (int?)x.Quantity) ?? 0
                })
                .ToListAsync(cancellationToken);

            var lowStockItems = stockBySku
                .Where(x => x.AvailableQty <= LowStockThreshold)
                .OrderBy(x => x.AvailableQty)
                .ThenBy(x => x.ProductName)
                .Take(8)
                .Select(x => new DashboardLowStockItem
                {
                    ProductVariantId = x.ID_ProductVariant,
                    Sku = x.SKU ?? string.Empty,
                    ProductName = x.ProductName ?? string.Empty,
                    AvailableQty = x.AvailableQty,
                    ReorderLevel = LowStockThreshold
                })
                .ToList();

            var statusRows = await activeOrders
                .GroupBy(o => string.IsNullOrWhiteSpace(o.OrderStatus) ? "Unknown" : o.OrderStatus)
                .Select(g => new DashboardNamedCount
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync(cancellationToken);

            var orderTrendRaw = await activeOrders
                .Where(o => o.OrderDate != null && o.OrderDate.Value.Date >= trendFrom)
                .GroupBy(o => o.OrderDate!.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(x => x.TotalAmount)
                })
                .ToListAsync(cancellationToken);

            var posTrendRaw = await sales
                .Where(s => s.SaleDate.Date >= trendFrom)
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(x => x.TotalAmount)
                })
                .ToListAsync(cancellationToken);

            var orderTrendMap = orderTrendRaw.ToDictionary(x => x.Date.Date, x => x);
            var posTrendMap = posTrendRaw.ToDictionary(x => x.Date.Date, x => x);
            var orderTrend = new List<DashboardDailyPoint>();
            var posSalesTrend = new List<DashboardDailyPoint>();
            for (var d = 0; d < TrendDays; d++)
            {
                var day = trendFrom.AddDays(d).Date;
                orderTrendMap.TryGetValue(day, out var oPoint);
                posTrendMap.TryGetValue(day, out var pPoint);
                orderTrend.Add(new DashboardDailyPoint
                {
                    Date = day.ToString("dd MMM"),
                    Count = oPoint?.Count ?? 0,
                    Amount = oPoint?.Amount ?? 0m
                });
                posSalesTrend.Add(new DashboardDailyPoint
                {
                    Date = day.ToString("dd MMM"),
                    Count = pPoint?.Count ?? 0,
                    Amount = pPoint?.Amount ?? 0m
                });
            }

            var topProducts = await (
                from oi in _db.OrderItems.AsNoTracking()
                join o in activeOrders on oi.FK_Order equals o.ID_Order
                group oi by oi.ProductName into g
                select new DashboardNamedAmount
                {
                    Name = g.Key,
                    Quantity = g.Sum(x => x.Quantity),
                    Amount = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(8)
                .ToListAsync(cancellationToken);

            var recentOrders = await (
                from o in orders
                join u in _db.Users.AsNoTracking() on o.FK_User equals u.ID_User into uj
                from u in uj.DefaultIfEmpty()
                orderby o.OrderDate descending, o.ID_Order descending
                select new DashboardRecentOrder
                {
                    OrderId = o.ID_Order,
                    OrderNumber = o.OrderNumber
                        ?? ("SC" + o.ID_Order.ToString("D6")),
                    CustomerName = !string.IsNullOrWhiteSpace(o.ReceiverName)
                        ? o.ReceiverName!
                        : (u != null ? (u.FullName ?? u.Email) : "Customer"),
                    Status = o.Cancelled == true ? "Cancelled" : o.OrderStatus,
                    TotalAmount = o.TotalAmount,
                    OrderDate = o.OrderDate,
                    Cancelled = o.Cancelled == true
                })
                .Take(8)
                .ToListAsync(cancellationToken);

            return new DashboardSummary
            {
                Kpis = new DashboardKpis
                {
                    TotalOrders = totalOrders,
                    OrdersToday = ordersToday,
                    PendingOrders = pendingOrders,
                    OnlineRevenue = onlineRevenue,
                    OnlineRevenueToday = onlineRevenueToday,
                    PosRevenue = posRevenue,
                    ActiveProducts = activeProducts,
                    ActiveSkus = activeSkus,
                    Customers = customers,
                    LowStockSkuCount = stockBySku.Count(x => x.AvailableQty <= LowStockThreshold),
                    SellOnlineSkuCount = sellOnlineSkuCount
                },
                OrderStatusCounts = statusRows,
                OrderTrend = orderTrend,
                PosSalesTrend = posSalesTrend,
                TopProducts = topProducts,
                LowStockItems = lowStockItems,
                RecentOrders = recentOrders,
                GeneratedAt = DateTime.Now
            };
        }
    }
}
