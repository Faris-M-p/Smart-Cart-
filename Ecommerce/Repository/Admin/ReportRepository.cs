using Ecommerce.DataAccess;
using Ecommerce.Interface.Admin;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.ReportModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class ReportRepository : IReportInterface
    {
        private readonly EcommerceDbContext _db;

        public ReportRepository(EcommerceDbContext db)
        {
            _db = db;
        }

        // ─── Dashboard Stats ─────────────────────────────────────────────
        public async Task<DashboardStats> GetDashboardStatsAsync()
        {
            var now        = DateTime.UtcNow;
            var thisMonth  = new DateTime(now.Year, now.Month, 1);
            var prevMonth  = thisMonth.AddMonths(-1);
            var last30     = now.Date.AddDays(-29);

            // Run all counts in parallel
            var allOrdersTask      = _db.Orders.AsNoTracking().Where(o => o.Cancelled != true).ToListAsync();
            var customersTask      = _db.Users.AsNoTracking().CountAsync(u => u.Cancelled != true && !u.IsAdmin);
            var productsTask       = _db.Products.AsNoTracking().CountAsync(p => !p.Cancelled);
            var pendingOrdersTask  = _db.Orders.AsNoTracking().CountAsync(o => o.Cancelled != true && o.OrderStatus == "Placed");

            // Low stock: sum per variant < 5
            var stockAggTask = _db.Stock.AsNoTracking()
                                   .Where(s => !s.Cancelled)
                                   .GroupBy(s => s.FK_ProductVariant)
                                   .Select(g => new { FK_ProductVariant = g.Key, Total = g.Sum(x => x.Quantity) })
                                   .ToListAsync();

            // Recent 10 orders
            var recentOrdersTask = (
                from o in _db.Orders.AsNoTracking().Where(o => o.Cancelled != true)
                join u in _db.Users.AsNoTracking() on o.FK_User equals u.ID_User into uj
                from u in uj.DefaultIfEmpty()
                orderby o.OrderDate descending
                select new RecentOrderItem
                {
                    OrderNumber  = o.OrderNumber ?? ("SC" + o.ID_Order.ToString("D6")),
                    CustomerName = u != null ? (u.FullName ?? u.UserName) : (o.ReceiverName ?? "Guest"),
                    TotalAmount  = o.TotalAmount,
                    OrderStatus  = o.OrderStatus ?? "-",
                    OrderDate    = o.OrderDate ?? DateTime.MinValue
                }
            ).Take(10).ToListAsync();

            await Task.WhenAll(allOrdersTask, customersTask, productsTask, pendingOrdersTask, stockAggTask, recentOrdersTask);

            var allOrders   = allOrdersTask.Result;
            var stockAgg    = stockAggTask.Result;
            var lowStock    = stockAgg.Count(s => s.Total <= 5);

            var thisMonthOrders = allOrders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value >= thisMonth).ToList();
            var prevMonthOrders = allOrders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value >= prevMonth && o.OrderDate.Value < thisMonth).ToList();
            var last30Orders    = allOrders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date >= last30).ToList();

            return new DashboardStats
            {
                TotalRevenue     = allOrders.Sum(o => o.TotalAmount),
                TotalOrders      = allOrders.Count,
                TotalCustomers   = customersTask.Result,
                TotalProducts    = productsTask.Result,
                RevenueThisMonth = thisMonthOrders.Sum(o => o.TotalAmount),
                OrdersThisMonth  = thisMonthOrders.Count,
                RevenuePrevMonth = prevMonthOrders.Sum(o => o.TotalAmount),
                OrdersPrevMonth  = prevMonthOrders.Count,
                PendingOrders    = pendingOrdersTask.Result,
                LowStockProducts = lowStock,

                Last30DaySales = last30Orders
                    .GroupBy(o => o.OrderDate!.Value.Date)
                    .Select(g => new DailySummary
                    {
                        Date  = g.Key.ToString("yyyy-MM-dd"),
                        Count = g.Count(),
                        Total = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(d => d.Date)
                    .ToList(),

                OrdersByStatus = allOrders
                    .GroupBy(o => o.OrderStatus ?? "Unknown")
                    .Select(g => new StatusSummary
                    {
                        Status = g.Key,
                        Count  = g.Count(),
                        Total  = g.Sum(o => o.TotalAmount)
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList(),

                RecentOrders = recentOrdersTask.Result
            };
        }

        // ─── Sales Report ────────────────────────────────────────────────
        public async Task<SalesReportSummary> GetSalesReportAsync(SalesReportInput input)
        {
            var query = _db.Orders.AsNoTracking()
                           .Where(o => o.Cancelled != true);

            if (input.FromDate.HasValue)
                query = query.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date >= input.FromDate.Value.Date);
            if (input.ToDate.HasValue)
                query = query.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date <= input.ToDate.Value.Date);

            var orders = await query.ToListAsync();

            var summary = new SalesReportSummary
            {
                TotalOrders = orders.Count,
                TotalSales  = orders.Sum(o => o.TotalAmount),

                PaymentMethodSummary = orders
                    .GroupBy(o => o.PaymentMethod ?? "Unknown")
                    .Select(g => new MethodSummary
                    {
                        Method = g.Key,
                        Count  = g.Count(),
                        Total  = g.Sum(o => o.TotalAmount)
                    }).ToList(),

                OrderStatusSummary = orders
                    .GroupBy(o => o.OrderStatus ?? "Unknown")
                    .Select(g => new StatusSummary
                    {
                        Status = g.Key,
                        Count  = g.Count(),
                        Total  = g.Sum(o => o.TotalAmount)
                    }).ToList(),

                DailySales = orders
                    .GroupBy(o => o.OrderDate.HasValue ? o.OrderDate.Value.Date : DateTime.MinValue.Date)
                    .Select(g => new DailySummary
                    {
                        Date  = g.Key.ToString("yyyy-MM-dd"),
                        Count = g.Count(),
                        Total = g.Sum(o => o.TotalAmount)
                    })
                    .OrderBy(d => d.Date)
                    .ToList()
            };

            return summary;
        }

        // ─── Order Report ────────────────────────────────────────────────
        public async Task<TableOutput<OrderReportItem>> GetOrderReportAsync(OrderReportInput input)
        {
            // Build the base order query
            var ordersQ = _db.Orders.AsNoTracking()
                             .Where(o => o.Cancelled != true);

            if (input.FromDate.HasValue)
                ordersQ = ordersQ.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date >= input.FromDate.Value.Date);
            if (input.ToDate.HasValue)
                ordersQ = ordersQ.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date <= input.ToDate.Value.Date);
            if (!string.IsNullOrWhiteSpace(input.OrderStatus))
                ordersQ = ordersQ.Where(o => o.OrderStatus == input.OrderStatus);
            if (!string.IsNullOrWhiteSpace(input.PaymentMethod))
                ordersQ = ordersQ.Where(o => o.PaymentMethod == input.PaymentMethod);
            if (!string.IsNullOrWhiteSpace(input.SearchText))
                ordersQ = ordersQ.Where(o =>
                    (o.OrderNumber != null && o.OrderNumber.Contains(input.SearchText)) ||
                    (o.ReceiverName != null && o.ReceiverName.Contains(input.SearchText)));

            // Join with users for the customer name
            var joined = from o in ordersQ
                         join u in _db.Users.AsNoTracking() on o.FK_User equals u.ID_User into uj
                         from u in uj.DefaultIfEmpty()
                         // latest payment per order
                         join p in _db.Payments.AsNoTracking() on o.ID_Order equals p.FK_Order into pj
                         from p in pj.OrderByDescending(x => x.ID_Payment).Take(1).DefaultIfEmpty()
                         select new OrderReportItem
                         {
                             OrderNumber   = o.OrderNumber ?? ("SC" + o.ID_Order.ToString("D6")),
                             CustomerName  = u != null ? (u.FullName ?? u.UserName) : (o.ReceiverName ?? "Guest"),
                             OrderDate     = o.OrderDate ?? DateTime.MinValue,
                             TotalAmount   = o.TotalAmount,
                             PaymentMethod = o.PaymentMethod ?? "-",
                             PaymentStatus = p != null ? (p.PaymentStatus ?? "Pending") : "Pending",
                             OrderStatus   = o.OrderStatus ?? "-"
                         };

            // Filter by payment status after join (avoids groupBy issues in EF)
            if (!string.IsNullOrWhiteSpace(input.PaymentStatus))
                joined = joined.Where(x => x.PaymentStatus == input.PaymentStatus);

            int totalRecords = await joined.CountAsync();

            var items = await joined
                .OrderByDescending(o => o.OrderDate)
                .Skip((input.PageIndex - 1) * input.PageSize)
                .Take(input.PageSize)
                .ToListAsync();

            return new TableOutput<OrderReportItem>
            {
                TableData     = items,
                TableSettings = new TableOutput_Settings
                {
                    TotalCount = totalRecords,
                    PageIndex  = input.PageIndex,
                    PageSize   = input.PageSize
                }
            };
        }

        // ─── Product Report ──────────────────────────────────────────────
        public async Task<TableOutput<ProductReportItem>> GetProductReportAsync(ProductReportInput input)
        {
            // Aggregate sales per (product, variant)
            var salesAgg = await (
                from oi in _db.OrderItems.AsNoTracking()
                join o  in _db.Orders.AsNoTracking().Where(x => x.Cancelled != true) on oi.FK_Order equals o.ID_Order
                group oi by new { oi.FK_Product, oi.FK_ProductVariant } into g
                select new
                {
                    FK_Product        = g.Key.FK_Product,
                    FK_ProductVariant = g.Key.FK_ProductVariant,
                    QuantitySold      = g.Sum(x => x.Quantity),
                    SalesAmount       = g.Sum(x => x.LineTotal)
                }
            ).ToListAsync();

            // Aggregate current stock per variant (sum all non-cancelled batches)
            var stockAgg = await (
                from s in _db.Stock.AsNoTracking().Where(s => !s.Cancelled)
                group s by s.FK_ProductVariant into g
                select new { FK_ProductVariant = g.Key, TotalQty = g.Sum(x => x.Quantity) }
            ).ToListAsync();

            // Product + variant + subcategory + category query
            var pvQuery = from p in _db.Products.AsNoTracking().Where(p => !p.Cancelled)
                          join sc in _db.SubCategories.AsNoTracking() on p.FK_SubCategory equals sc.ID_SubCategory into scj
                          from sc in scj.DefaultIfEmpty()
                          join c  in _db.Categories.AsNoTracking()   on (sc != null ? sc.FK_Category : 0) equals c.ID_Category into cj
                          from c  in cj.DefaultIfEmpty()
                          join pv in _db.ProductVariants.AsNoTracking().Where(pv => !pv.Cancelled) on p.ID_Product equals pv.FK_Product
                          select new { p, sc, c, pv };

            if (!string.IsNullOrWhiteSpace(input.SearchText))
                pvQuery = pvQuery.Where(x => x.p.Name.Contains(input.SearchText));

            var pvList = await pvQuery.ToListAsync();

            // Join in memory (sales + stock aggregates are already materialised)
            var result = pvList.Select(x =>
            {
                var sale  = salesAgg.FirstOrDefault(s => s.FK_Product == x.p.ID_Product && s.FK_ProductVariant == x.pv.ID_ProductVariant);
                var stock = stockAgg.FirstOrDefault(s => s.FK_ProductVariant == x.pv.ID_ProductVariant);
                return new ProductReportItem
                {
                    ProductName  = x.p.Name + (!string.IsNullOrEmpty(x.pv.VariantLabel) ? " - " + x.pv.VariantLabel : ""),
                    CategoryName = x.c?.Name ?? "-",
                    QuantitySold = sale?.QuantitySold ?? 0,
                    SalesAmount  = sale?.SalesAmount  ?? 0,
                    CurrentStock = stock?.TotalQty    ?? 0
                };
            })
            .OrderByDescending(r => r.QuantitySold)
            .ToList();

            int total = result.Count;
            var page  = result
                .Skip((input.PageIndex - 1) * input.PageSize)
                .Take(input.PageSize)
                .ToList();

            return new TableOutput<ProductReportItem>
            {
                TableData     = page,
                TableSettings = new TableOutput_Settings
                {
                    TotalCount = total,
                    PageIndex  = input.PageIndex,
                    PageSize   = input.PageSize
                }
            };
        }

        // ─── Customer Report ─────────────────────────────────────────────
        public async Task<TableOutput<CustomerReportItem>> GetCustomerReportAsync(CustomerReportInput input)
        {
            // Aggregate orders per user
            var orderAgg = await (
                from o in _db.Orders.AsNoTracking().Where(x => x.Cancelled != true)
                group o by o.FK_User into g
                select new
                {
                    FK_User             = g.Key,
                    TotalOrders         = g.Count(),
                    TotalPurchaseAmount = g.Sum(x => x.TotalAmount),
                    LastOrderDate       = g.Max(x => x.OrderDate)
                }
            ).ToListAsync();

            // All non-admin, non-deleted users
            var usersQ = _db.Users.AsNoTracking()
                             .Where(u => u.Cancelled != true && !u.IsAdmin);

            if (!string.IsNullOrWhiteSpace(input.SearchText))
                usersQ = usersQ.Where(u =>
                    (u.FullName != null && u.FullName.Contains(input.SearchText)) ||
                    u.Email.Contains(input.SearchText) ||
                    u.UserName.Contains(input.SearchText));

            var users = await usersQ.ToListAsync();

            var result = users.Select(u =>
            {
                var agg = orderAgg.FirstOrDefault(o => o.FK_User == u.ID_User);
                return new CustomerReportItem
                {
                    CustomerName        = u.FullName ?? u.UserName,
                    Email               = u.Email,
                    TotalOrders         = agg?.TotalOrders         ?? 0,
                    TotalPurchaseAmount = agg?.TotalPurchaseAmount ?? 0,
                    LastOrderDate       = agg?.LastOrderDate
                };
            })
            .OrderByDescending(r => r.TotalOrders)
            .ToList();

            int total = result.Count;
            var page  = result
                .Skip((input.PageIndex - 1) * input.PageSize)
                .Take(input.PageSize)
                .ToList();

            return new TableOutput<CustomerReportItem>
            {
                TableData     = page,
                TableSettings = new TableOutput_Settings
                {
                    TotalCount = total,
                    PageIndex  = input.PageIndex,
                    PageSize   = input.PageSize
                }
            };
        }
    }
}
