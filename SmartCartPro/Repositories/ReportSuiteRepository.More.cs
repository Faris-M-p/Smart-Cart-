using Microsoft.EntityFrameworkCore;
using SmartCartPro.Helpers;
using SmartCartPro.Models;

namespace SmartCartPro.Repositories;

public partial class ReportSuiteRepository
{
    private async Task<OutputReportTable> PurchaseBills(InputReportQuery input, CancellationToken ct)
    {
        var q = _db.Purchases.AsNoTracking()
            .Where(p => p.PurchaseDate >= input.FromLocal && p.PurchaseDate <= input.ToLocal);
        if (input.StoreIds is not null)
            q = q.Where(p => input.StoreIds.Contains(p.FkStore));
        if (input.FkSupplier is > 0)
            q = q.Where(p => p.FkSupplier == input.FkSupplier);
        var status = (input.Status ?? "all").Trim().ToLowerInvariant();
        if (status == "cancelled")
            q = q.Where(p => p.Cancelled || p.Status == "cancelled");
        else if (status is "posted" or "partially_received" or "received" or "draft")
            q = q.Where(p => !p.Cancelled && p.Status == status);

        var active = q.Where(p => !p.Cancelled && p.Status != "cancelled");
        var count = await active.CountAsync(ct);
        var gross = count == 0 ? 0 : await active.SumAsync(p => p.SubTotal, ct);
        var discount = count == 0 ? 0 : await active.SumAsync(p => p.DiscountAmount, ct);
        var tax = count == 0 ? 0 : await active.SumAsync(p => p.TaxAmount, ct);
        var net = count == 0 ? 0 : await active.SumAsync(p => p.TotalAmount, ct);
        var totalCount = await q.CountAsync(ct);
        var (page, size) = Paging(input);
        var pageRows = await q.OrderByDescending(p => p.PurchaseDate).Skip((page - 1) * size).Take(size)
            .Select(p => new { p.IdPurchase, p.InvoiceNumber, p.PurchaseDate, p.FkSupplier, p.SubTotal, p.DiscountAmount, p.TaxAmount, p.TotalAmount, p.Status, p.Cancelled })
            .ToListAsync(ct);
        var ids = pageRows.Select(p => p.IdPurchase).ToList();
        var lines = ids.Count == 0
            ? new Dictionary<int, int>()
            : (await _db.PurchaseDetails.AsNoTracking().Where(d => ids.Contains(d.FkPurchase))
                .GroupBy(d => d.FkPurchase).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct))
                .ToDictionary(x => x.Key, x => x.Count);
        var supplierIds = pageRows.Select(p => p.FkSupplier).Distinct().ToList();
        var suppliers = supplierIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Suppliers.AsNoTracking().Where(s => supplierIds.Contains(s.IdSupplier)).ToDictionaryAsync(s => s.IdSupplier, s => s.SupplierName, ct);

        var rows = pageRows.Select(p =>
        {
            var muted = p.Cancelled || p.Status == "cancelled";
            return new[]
            {
                Link(string.IsNullOrWhiteSpace(p.InvoiceNumber) ? "PO " + p.IdPurchase : p.InvoiceNumber, "/Purchase/Details/" + p.IdPurchase, muted),
                C(ReportFormat.Day(p.PurchaseDate), muted),
                Link(suppliers.GetValueOrDefault(p.FkSupplier), "/Reports/PurchaseAnalysis?fkSupplier=" + p.FkSupplier + "&groupBy=supplier&" + Dates(input) + StoreQ(input), muted),
                C(ReportFormat.Count(lines.GetValueOrDefault(p.IdPurchase)), muted),
                C(ReportFormat.Money(p.SubTotal), muted),
                C(ReportFormat.Money(p.DiscountAmount), muted),
                C(ReportFormat.Money(p.TaxAmount), muted),
                C(ReportFormat.Money(p.TotalAmount), muted),
                C(muted ? "Cancelled" : p.Status, muted)
            };
        }).ToList();

        return new OutputReportTable
        {
            Columns = new()
            {
                Col("Bill no."), Col("Date"), Col("Supplier"), Col("Items", "end"),
                Col("Gross", "end"), Col("Discount", "end"), Col("Tax", "end"), Col("Net", "end"), Col("Status")
            },
            Rows = rows,
            TotalCount = totalCount,
            Totals = new[]
            {
                C("Totals"), C(""), C(""), C(ReportFormat.Count(count)),
                C(ReportFormat.Money(gross)), C(ReportFormat.Money(discount)), C(ReportFormat.Money(tax)), C(ReportFormat.Money(net)), C("")
            },
            Kpis = new()
            {
                Kpi("Bills", ReportFormat.Count(count)),
                Kpi("Net", ReportFormat.Money(net)),
                Kpi("Tax", ReportFormat.Money(tax))
            },
            Notice = "The purchase bill number is the supplier invoice stored on the purchase. A separate GRN number is not a column; received quantity is on the purchase screen."
        };
    }

    private async Task<OutputReportTable> PurchaseAnalysis(InputReportQuery input, CancellationToken ct)
    {
        var itemWise = string.Equals(input.GroupBy, "item", StringComparison.OrdinalIgnoreCase);
        var headers = _db.Purchases.AsNoTracking()
            .Where(p => !p.Cancelled && p.Status != "cancelled" && p.PurchaseDate >= input.FromLocal && p.PurchaseDate <= input.ToLocal);
        if (input.StoreIds is not null)
            headers = headers.Where(p => input.StoreIds.Contains(p.FkStore));
        if (input.FkSupplier is > 0)
            headers = headers.Where(p => p.FkSupplier == input.FkSupplier);

        if (!itemWise)
        {
            var facts = await (
                from p in headers
                join s in _db.Suppliers.AsNoTracking() on p.FkSupplier equals s.IdSupplier
                group p by new { p.FkSupplier, s.SupplierName }
                into g
                select new
                {
                    g.Key.FkSupplier,
                    g.Key.SupplierName,
                    Bills = g.Count(),
                    Net = g.Sum(x => x.TotalAmount)
                }).ToListAsync(ct);

            var qtyRows = await (
                from d in _db.PurchaseDetails.AsNoTracking()
                join p in headers on d.FkPurchase equals p.IdPurchase
                group d by p.FkSupplier into g
                select new { Supplier = g.Key, Qty = g.Sum(x => x.Quantity) }).ToListAsync(ct);
            var qtyMap = qtyRows.ToDictionary(x => x.Supplier, x => x.Qty);
            var ordered = facts.OrderByDescending(x => x.Net).ToList();
            var (page, size) = Paging(input);
            var rows = Slice(ordered, page, size).Select(x => new[]
            {
                Link(x.SupplierName, "/Reports/PurchaseBills?fkSupplier=" + x.FkSupplier + "&" + Dates(input) + StoreQ(input)),
                C(ReportFormat.Count(x.Bills)),
                C(ReportFormat.Qty(qtyMap.GetValueOrDefault(x.FkSupplier))),
                C(ReportFormat.Money(x.Net))
            }).ToList();
            return new OutputReportTable
            {
                Columns = new() { Col("Supplier"), Col("Bills", "end"), Col("Qty", "end"), Col("Net value", "end") },
                Rows = rows,
                TotalCount = ordered.Count,
                Totals = new[]
                {
                    C("Totals"), C(ReportFormat.Count(ordered.Sum(x => x.Bills))),
                    C(ReportFormat.Qty(qtyMap.Values.Sum())), C(ReportFormat.Money(ordered.Sum(x => x.Net)))
                },
                Kpis = new() { Kpi("Suppliers", ReportFormat.Count(ordered.Count)), Kpi("Net", ReportFormat.Money(ordered.Sum(x => x.Net))) }
            };
        }

        var lines = await (
            from d in _db.PurchaseDetails.AsNoTracking()
            join p in headers on d.FkPurchase equals p.IdPurchase
            join pr in _db.Products.AsNoTracking() on d.FkProduct equals pr.IdProduct
            where !(input.FkCategory > 0) || pr.FkCategory == input.FkCategory
            group d by new { d.FkProduct, Variant = d.FkProductVariant ?? 0, pr.ProductName }
            into g
            select new
            {
                g.Key.FkProduct,
                g.Key.Variant,
                g.Key.ProductName,
                Qty = g.Sum(x => x.Quantity),
                Value = g.Sum(x => x.LineTotal),
                Min = g.Min(x => x.UnitCost),
                Max = g.Max(x => x.UnitCost),
                Suppliers = g.Select(x => x.FkPurchase).Distinct().Count()
            }).ToListAsync(ct);

        var purchaseSuppliers = await (
            from d in _db.PurchaseDetails.AsNoTracking()
            join p in headers on d.FkPurchase equals p.IdPurchase
            group p by new { d.FkProduct, Variant = d.FkProductVariant ?? 0 }
            into g
            select new { g.Key.FkProduct, g.Key.Variant, Suppliers = g.Select(x => x.FkSupplier).Distinct().Count() }
        ).ToListAsync(ct);
        var supMap = purchaseSuppliers.ToDictionary(x => (x.FkProduct, x.Variant), x => x.Suppliers);
        var orderedItems = lines.OrderByDescending(x => x.Value).ToList();
        var (pageNo, pageSize) = Paging(input);
        var itemRows = Slice(orderedItems, pageNo, pageSize).Select(x =>
        {
            var avg = x.Qty == 0 ? 0 : Math.Round(x.Value / x.Qty, 2, MidpointRounding.AwayFromZero);
            return new[]
            {
                Link(x.ProductName, "/Reports/ItemLedger?fkProduct=" + x.FkProduct + (x.Variant > 0 ? "&fkProductVariant=" + x.Variant : "") + "&" + Dates(input) + StoreQ(input)),
                C(ReportFormat.Qty(x.Qty)),
                C(ReportFormat.Money(avg)),
                C(ReportFormat.Money(x.Min)),
                C(ReportFormat.Money(x.Max)),
                C(ReportFormat.Money(x.Value)),
                C(ReportFormat.Count(supMap.GetValueOrDefault((x.FkProduct, x.Variant))))
            };
        }).ToList();

        return new OutputReportTable
        {
            Columns = new()
            {
                Col("Item"), Col("Qty received", "end"), Col("Avg cost", "end"), Col("Min cost", "end"),
                Col("Max cost", "end"), Col("Value", "end"), Col("Suppliers", "end")
            },
            Rows = itemRows,
            TotalCount = orderedItems.Count,
            Totals = new[]
            {
                C("Totals"), C(ReportFormat.Qty(orderedItems.Sum(x => x.Qty))), C(""), C(""), C(""),
                C(ReportFormat.Money(orderedItems.Sum(x => x.Value))), C("")
            },
            Kpis = new() { Kpi("Items", ReportFormat.Count(orderedItems.Count)), Kpi("Value", ReportFormat.Money(orderedItems.Sum(x => x.Value))) },
            Notice = "Received quantity here is the purchase line quantity, which is the ordered quantity. Goods actually received are posted as stock movements and can be less on a partial receipt."
        };
    }

    private async Task<OutputReportTable> GstSummary(InputReportQuery input, CancellationToken ct)
    {
        var sales = await (
            from d in _db.SaleDetails.AsNoTracking()
            join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
            join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
            join t in _db.Taxes.AsNoTracking() on p.FkTax equals t.IdTax into tj
            from t in tj.DefaultIfEmpty()
            where !s.Cancelled && s.Status == "completed"
                && s.SaleDate >= input.FromUtc && s.SaleDate < input.ToUtcExclusive
                && (input.StoreIds == null || input.StoreIds.Contains(s.FkStore))
            group d by new { Rate = t == null ? 0m : t.TaxRate, Name = t == null ? "No tax" : t.TaxName }
            into g
            select new
            {
                g.Key.Rate,
                g.Key.Name,
                Taxable = g.Sum(x => x.LineTotal - x.TaxAmount),
                Tax = g.Sum(x => x.TaxAmount)
            }).ToListAsync(ct);

        var returns = await (
            from d in _db.SalesReturnDetails.AsNoTracking()
            join r in _db.SalesReturns.AsNoTracking() on d.FkSalesReturn equals r.IdSalesReturn
            join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
            join t in _db.Taxes.AsNoTracking() on p.FkTax equals t.IdTax into tj
            from t in tj.DefaultIfEmpty()
            where !r.Cancelled
                && r.ReturnDate >= input.FromUtc && r.ReturnDate < input.ToUtcExclusive
                && (input.StoreIds == null || input.StoreIds.Contains(r.FkStore))
            group d by t == null ? 0m : t.TaxRate
            into g
            select new { Rate = g.Key, Value = g.Sum(x => x.LineTotal) }).ToListAsync(ct);
        var returnMap = returns.ToDictionary(x => x.Rate, x => x.Value);

        var rates = sales.Select(s => s.Rate).Union(returns.Select(r => r.Rate)).OrderBy(r => r).ToList();
        var rows = rates.Select(rate =>
        {
            var sale = sales.FirstOrDefault(s => s.Rate == rate);
            var ret = returnMap.GetValueOrDefault(rate);
            var taxable = sale?.Taxable ?? 0;
            return new[]
            {
                C(sale?.Name ?? "No tax"),
                C(rate.ToString("0.####") + "%"),
                C(ReportFormat.Money(taxable)),
                C(ReportFormat.Money(sale?.Tax ?? 0)),
                C(ReportFormat.Money(ret)),
                C(ReportFormat.Money(taxable - ret))
            };
        }).ToList();

        return new OutputReportTable
        {
            Columns = new()
            {
                Col("Tax"), Col("Rate", "end"), Col("Taxable sales", "end"), Col("Tax amount", "end"),
                Col("Return value", "end"), Col("Net taxable", "end")
            },
            Rows = rows,
            TotalCount = rows.Count,
            Totals = new[]
            {
                C("Totals"), C(""),
                C(ReportFormat.Money(sales.Sum(x => x.Taxable))),
                C(ReportFormat.Money(sales.Sum(x => x.Tax))),
                C(ReportFormat.Money(returns.Sum(x => x.Value))),
                C(ReportFormat.Money(sales.Sum(x => x.Taxable) - returns.Sum(x => x.Value)))
            },
            Kpis = new()
            {
                Kpi("Taxable", ReportFormat.Money(sales.Sum(x => x.Taxable))),
                Kpi("Tax", ReportFormat.Money(sales.Sum(x => x.Tax))),
                Kpi("Returns", ReportFormat.Money(returns.Sum(x => x.Value)))
            },
            Notice = "Tax is a single rate on the product. CGST, SGST, IGST, cess and HSN are not stored. Return lines do not store tax, so tax amount is sales tax only and net taxable subtracts the return line value."
        };
    }

    private async Task<OutputReportTable> ProfitMargin(InputReportQuery input, CancellationToken ct)
    {
        if (!input.CanSeeProfit)
        {
            return new OutputReportTable { Notice = "Cost and profit need the reports.profit permission." };
        }

        var group = (input.GroupBy ?? "item").Trim().ToLowerInvariant();
        var sold = await LoadSoldAsync(input, ct);
        var returns = await LoadReturnsAsync(input, ct);
        var labels = await SkuLabelsAsync(sold.Select(s => (s.ProductId, s.VariantId)).Union(returns.Keys).ToList(), ct);

        if (group == "bill")
        {
            var bills = await (
                from d in _db.SaleDetails.AsNoTracking()
                join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
                join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
                where !s.Cancelled && s.Status == "completed"
                    && s.SaleDate >= input.FromUtc && s.SaleDate < input.ToUtcExclusive
                    && (input.StoreIds == null || input.StoreIds.Contains(s.FkStore))
                group new { d, p } by new { s.IdSale, s.InvoiceNumber }
                into g
                select new
                {
                    g.Key.IdSale,
                    g.Key.InvoiceNumber,
                    Qty = g.Sum(x => x.d.Quantity),
                    Net = g.Sum(x => x.d.LineTotal - x.d.TaxAmount),
                    Cost = g.Sum(x => x.d.Quantity * x.p.CostPrice)
                }).ToListAsync(ct);

            var retBySale = await (
                from d in _db.SalesReturnDetails.AsNoTracking()
                join r in _db.SalesReturns.AsNoTracking() on d.FkSalesReturn equals r.IdSalesReturn
                join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
                where !r.Cancelled && bills.Select(b => b.IdSale).Contains(r.FkSale)
                group new { d, p } by r.FkSale
                into g
                select new { Sale = g.Key, Qty = g.Sum(x => x.d.Quantity), Value = g.Sum(x => x.d.LineTotal), Cost = g.Sum(x => x.d.Quantity * x.p.CostPrice) }
            ).ToListAsync(ct);
            var retMap = retBySale.ToDictionary(x => x.Sale);

            var built = bills.Select(b =>
            {
                retMap.TryGetValue(b.IdSale, out var ret);
                var qty = b.Qty - (ret?.Qty ?? 0);
                var net = b.Net - (ret?.Value ?? 0);
                var cost = b.Cost - (ret?.Cost ?? 0);
                var profit = net - cost;
                var margin = net == 0 ? 0 : Math.Round(profit * 100m / net, 2, MidpointRounding.AwayFromZero);
                return new { b.IdSale, b.InvoiceNumber, qty, net, cost, profit, margin };
            }).OrderByDescending(x => x.profit).ToList();

            var (page, size) = Paging(input);
            return ProfitTable(built.Select(x => (x.InvoiceNumber, "/Sales/InvoiceDetails/" + x.IdSale, x.qty, x.net, x.cost, x.profit, x.margin)).ToList(), page, size, "Bill");
        }

        var itemRows = sold.Select(s => (Product: s.ProductId, Variant: s.VariantId, Category: s.CategoryId, Qty: s.Qty, Net: s.Net - s.Tax, Cost: s.Qty * s.Cost))
            .Concat(returns.Select(r => (
                Product: r.Key.Product,
                Variant: r.Key.Variant,
                Category: sold.FirstOrDefault(s => s.ProductId == r.Key.Product && s.VariantId == r.Key.Variant)?.CategoryId ?? 0,
                Qty: -r.Value.Qty,
                Net: -r.Value.Value,
                Cost: -(r.Value.Qty * (sold.FirstOrDefault(s => s.ProductId == r.Key.Product && s.VariantId == r.Key.Variant)?.Cost ?? 0)))))
            .ToList();

        if (group == "category")
        {
            var catIds = itemRows.Select(x => x.Category).Where(id => id > 0).Distinct().ToList();
            var cats = catIds.Count == 0
                ? new Dictionary<int, string>()
                : await _db.Categories.AsNoTracking().Where(c => catIds.Contains(c.IdCategory)).ToDictionaryAsync(c => c.IdCategory, c => c.CategoryName, ct);
            var grouped = itemRows.GroupBy(x => x.Category).Select(g =>
            {
                var qty = g.Sum(x => x.Qty);
                var net = g.Sum(x => x.Net);
                var cost = g.Sum(x => x.Cost);
                var profit = net - cost;
                var margin = net == 0 ? 0 : Math.Round(profit * 100m / net, 2, MidpointRounding.AwayFromZero);
                var name = g.Key > 0 && cats.TryGetValue(g.Key, out var cn) ? cn : "Uncategorised";
                return (Name: name, Href: "/Reports/ItemSales?fkCategory=" + g.Key + "&" + Dates(input) + StoreQ(input), Qty: qty, Net: net, Cost: cost, Profit: profit, Margin: margin);
            }).OrderByDescending(x => x.Profit).ToList();
            var (page, size) = Paging(input);
            return ProfitTable(grouped, page, size, "Category");
        }

        var merged = itemRows.GroupBy(x => (Product: x.Product, Variant: x.Variant)).Select(g =>
        {
            var qty = g.Sum(x => x.Qty);
            var net = g.Sum(x => x.Net);
            var cost = g.Sum(x => x.Cost);
            var profit = net - cost;
            var margin = net == 0 ? 0 : Math.Round(profit * 100m / net, 2, MidpointRounding.AwayFromZero);
            labels.TryGetValue((g.Key.Product, g.Key.Variant), out var label);
            return (Name: label.Name ?? "Item", Href: "/Reports/ItemLedger?fkProduct=" + g.Key.Product + (g.Key.Variant > 0 ? "&fkProductVariant=" + g.Key.Variant : "") + "&" + Dates(input) + StoreQ(input), Qty: qty, Net: net, Cost: cost, Profit: profit, Margin: margin);
        }).OrderByDescending(x => x.Profit).ToList();
        var paging = Paging(input);
        return ProfitTable(merged, paging.Page, paging.Size, "Item");
    }

    private static OutputReportTable ProfitTable(
        List<(string Name, string Href, decimal Qty, decimal Net, decimal Cost, decimal Profit, decimal Margin)> rows,
        int page, int size, string firstColumn)
    {
        var pageRows = Slice(rows, page, size).Select(x => new[]
        {
            Link(x.Name, x.Href),
            C(ReportFormat.Qty(x.Qty)),
            C(ReportFormat.Money(x.Net)),
            C(ReportFormat.Money(x.Cost)),
            C(ReportFormat.Money(x.Profit)),
            C(ReportFormat.Pct(x.Margin))
        }).ToList();
        var net = rows.Sum(x => x.Net);
        var profit = rows.Sum(x => x.Profit);
        return new OutputReportTable
        {
            Columns = new()
            {
                Col(firstColumn), Col("Qty", "end"), Col("Net sales ex. tax", "end"),
                Col("Cost", "end"), Col("Gross profit", "end"), Col("Margin", "end")
            },
            Rows = pageRows,
            TotalCount = rows.Count,
            Totals = new[]
            {
                C("Totals"), C(ReportFormat.Qty(rows.Sum(x => x.Qty))), C(ReportFormat.Money(net)),
                C(ReportFormat.Money(rows.Sum(x => x.Cost))), C(ReportFormat.Money(profit)),
                C(ReportFormat.Pct(net == 0 ? 0 : Math.Round(profit * 100m / net, 2, MidpointRounding.AwayFromZero)))
            },
            Kpis = new()
            {
                Kpi("Net sales ex. tax", ReportFormat.Money(net)),
                Kpi("Cost", ReportFormat.Money(rows.Sum(x => x.Cost))),
                Kpi("Gross profit", ReportFormat.Money(profit))
            },
            Notice = "Cost is the current product cost, not the cost at the time of sale. Returns reduce quantity, sales and cost. Return lines do not store tax, so the full return amount is deducted from sales excluding tax."
        };
    }

    private async Task<OutputReportTable> DiscountReport(InputReportQuery input, CancellationToken ct)
    {
        var group = (input.GroupBy ?? "bill").Trim().ToLowerInvariant();
        if (group == "item")
        {
            var lines = await (
                from d in _db.SaleDetails.AsNoTracking()
                join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
                join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
                where !s.Cancelled && s.Status == "completed" && d.DiscountAmount > 0
                    && s.SaleDate >= input.FromUtc && s.SaleDate < input.ToUtcExclusive
                    && (input.StoreIds == null || input.StoreIds.Contains(s.FkStore))
                group d by new { d.FkProduct, p.ProductName }
                into g
                select new
                {
                    g.Key.FkProduct,
                    g.Key.ProductName,
                    Bills = g.Select(x => x.FkSale).Distinct().Count(),
                    Discount = g.Sum(x => x.DiscountAmount),
                    Gross = g.Sum(x => x.LineTotal - x.TaxAmount + x.DiscountAmount)
                }).ToListAsync(ct);
            var ordered = lines.OrderByDescending(x => x.Discount).ToList();
            var (page, size) = Paging(input);
            return new OutputReportTable
            {
                Columns = new() { Col("Item"), Col("Bills", "end"), Col("Gross", "end"), Col("Discount", "end"), Col("% of gross", "end") },
                Rows = Slice(ordered, page, size).Select(x => new[]
                {
                    Link(x.ProductName, "/Reports/ItemSales?fkProduct=" + x.FkProduct + "&" + Dates(input) + StoreQ(input)),
                    C(ReportFormat.Count(x.Bills)), C(ReportFormat.Money(x.Gross)), C(ReportFormat.Money(x.Discount)),
                    C(ReportFormat.Pct(x.Gross == 0 ? 0 : Math.Round(x.Discount * 100m / x.Gross, 2)))
                }).ToList(),
                TotalCount = ordered.Count,
                Totals = new[] { C("Totals"), C(""), C(ReportFormat.Money(ordered.Sum(x => x.Gross))), C(ReportFormat.Money(ordered.Sum(x => x.Discount))), C("") },
                Kpis = new() { Kpi("Discount", ReportFormat.Money(ordered.Sum(x => x.Discount))) }
            };
        }

        var sales = _db.Sales.AsNoTracking().Where(s => !s.Cancelled && s.Status == "completed" && s.DiscountAmount > 0
            && s.SaleDate >= input.FromUtc && s.SaleDate < input.ToUtcExclusive);
        if (input.StoreIds is not null)
            sales = sales.Where(s => input.StoreIds.Contains(s.FkStore));

        if (group == "cashier")
        {
            var facts = await sales.GroupBy(s => s.CreatedBy ?? 0)
                .Select(g => new { User = g.Key, Bills = g.Count(), Gross = g.Sum(x => x.SubTotal), Discount = g.Sum(x => x.DiscountAmount) })
                .ToListAsync(ct);
            var userIds = facts.Where(f => f.User > 0).Select(f => f.User).ToList();
            var users = userIds.Count == 0 ? new Dictionary<int, string>()
                : await _db.Users.AsNoTracking().Where(u => userIds.Contains(u.IdUser)).ToDictionaryAsync(u => u.IdUser, u => u.FullName, ct);
            var ordered = facts.OrderByDescending(x => x.Discount).ToList();
            var (page, size) = Paging(input);
            return new OutputReportTable
            {
                Columns = new() { Col("Cashier"), Col("Bills", "end"), Col("Gross", "end"), Col("Discount", "end"), Col("% of gross", "end") },
                Rows = Slice(ordered, page, size).Select(x => new[]
                {
                    Link(x.User > 0 && users.TryGetValue(x.User, out var name) ? name : "—", "/Reports/DayClose?fkUser=" + x.User + "&" + Dates(input) + StoreQ(input)),
                    C(ReportFormat.Count(x.Bills)), C(ReportFormat.Money(x.Gross)), C(ReportFormat.Money(x.Discount)),
                    C(ReportFormat.Pct(x.Gross == 0 ? 0 : Math.Round(x.Discount * 100m / x.Gross, 2)))
                }).ToList(),
                TotalCount = ordered.Count,
                Kpis = new() { Kpi("Discount", ReportFormat.Money(ordered.Sum(x => x.Discount))) }
            };
        }

        var totalCount = await sales.CountAsync(ct);
        var discount = totalCount == 0 ? 0 : await sales.SumAsync(s => s.DiscountAmount, ct);
        var gross = totalCount == 0 ? 0 : await sales.SumAsync(s => s.SubTotal, ct);
        var (pageNo, pageSize) = Paging(input);
        var pageRows = await sales.OrderByDescending(s => s.DiscountAmount).Skip((pageNo - 1) * pageSize).Take(pageSize)
            .Select(s => new { s.IdSale, s.InvoiceNumber, s.SaleDate, s.SubTotal, s.DiscountAmount }).ToListAsync(ct);
        return new OutputReportTable
        {
            Columns = new() { Col("Bill"), Col("Date"), Col("Gross", "end"), Col("Discount", "end"), Col("% of gross", "end") },
            Rows = pageRows.Select(s => new[]
            {
                Link(s.InvoiceNumber, "/Sales/InvoiceDetails/" + s.IdSale),
                C(ReportFormat.When(Local(s.SaleDate, input.TimeZone))),
                C(ReportFormat.Money(s.SubTotal)),
                C(ReportFormat.Money(s.DiscountAmount)),
                C(ReportFormat.Pct(s.SubTotal == 0 ? 0 : Math.Round(s.DiscountAmount * 100m / s.SubTotal, 2)))
            }).ToList(),
            TotalCount = totalCount,
            Totals = new[] { C("Totals"), C(""), C(ReportFormat.Money(gross)), C(ReportFormat.Money(discount)), C(ReportFormat.Pct(gross == 0 ? 0 : Math.Round(discount * 100m / gross, 2))) },
            Kpis = new() { Kpi("Bills", ReportFormat.Count(totalCount)), Kpi("Discount", ReportFormat.Money(discount)) }
        };
    }

    private async Task<OutputReportTable> CancelledBills(InputReportQuery input, CancellationToken ct)
    {
        var q = _db.Sales.AsNoTracking().Where(s => (s.Cancelled || s.Status == "cancelled")
            && s.SaleDate >= input.FromUtc && s.SaleDate < input.ToUtcExclusive);
        if (input.StoreIds is not null)
            q = q.Where(s => input.StoreIds.Contains(s.FkStore));
        if (input.FkUser is > 0)
            q = q.Where(s => s.CreatedBy == input.FkUser);
        var totalCount = await q.CountAsync(ct);
        var amount = totalCount == 0 ? 0 : await q.SumAsync(s => s.TotalAmount, ct);
        var (page, size) = Paging(input);
        var pageRows = await q.OrderByDescending(s => s.SaleDate).Skip((page - 1) * size).Take(size)
            .Select(s => new { s.IdSale, s.InvoiceNumber, s.SaleDate, s.TotalAmount, s.CancelledOn, s.CancelledReason, s.CreatedBy })
            .ToListAsync(ct);
        var userIds = pageRows.Where(s => s.CreatedBy != null).Select(s => s.CreatedBy!.Value).Distinct().ToList();
        var users = userIds.Count == 0 ? new Dictionary<int, string>()
            : await _db.Users.AsNoTracking().Where(u => userIds.Contains(u.IdUser)).ToDictionaryAsync(u => u.IdUser, u => u.FullName, ct);
        return new OutputReportTable
        {
            Columns = new() { Col("Bill"), Col("Date"), Col("Amount", "end"), Col("Cancelled at"), Col("Reason"), Col("Billed by") },
            Rows = pageRows.Select(s => new[]
            {
                Link(s.InvoiceNumber, "/Sales/InvoiceDetails/" + s.IdSale),
                C(ReportFormat.When(Local(s.SaleDate, input.TimeZone))),
                C(ReportFormat.Money(s.TotalAmount)),
                C(s.CancelledOn is DateTime at ? ReportFormat.When(Local(at, input.TimeZone)) : "—"),
                C(s.CancelledReason),
                C(s.CreatedBy is int uid && users.TryGetValue(uid, out var name) ? name : "—")
            }).ToList(),
            TotalCount = totalCount,
            Totals = new[] { C("Totals"), C(ReportFormat.Count(totalCount)), C(ReportFormat.Money(amount)), C(""), C(""), C("") },
            Kpis = new() { Kpi("Bills", ReportFormat.Count(totalCount)), Kpi("Amount", ReportFormat.Money(amount)) },
            Notice = "The user who cancelled the bill is not stored. Billed by is the cashier who created the invoice."
        };
    }

    private async Task<OutputReportTable> HourlySales(InputReportQuery input, CancellationToken ct)
    {
        var sales = await _db.Sales.AsNoTracking()
            .Where(s => !s.Cancelled && s.Status == "completed"
                && s.SaleDate >= input.FromUtc && s.SaleDate < input.ToUtcExclusive
                && (input.StoreIds == null || input.StoreIds.Contains(s.FkStore)))
            .Select(s => new { s.SaleDate, s.TotalAmount })
            .ToListAsync(ct);

        var byDow = string.Equals(input.GroupBy, "dow", StringComparison.OrdinalIgnoreCase);
        if (byDow)
        {
            var groups = sales.GroupBy(s => Local(s.SaleDate, input.TimeZone).DayOfWeek)
                .OrderBy(g => g.Key)
                .Select(g => new { Label = g.Key.ToString(), Bills = g.Count(), Net = g.Sum(x => x.TotalAmount) })
                .ToList();
            return new OutputReportTable
            {
                Columns = new() { Col("Day"), Col("Bills", "end"), Col("Net", "end") },
                Rows = groups.Select(g => new[] { C(g.Label), C(ReportFormat.Count(g.Bills)), C(ReportFormat.Money(g.Net)) }).ToList(),
                TotalCount = groups.Count,
                Totals = new[] { C("Totals"), C(ReportFormat.Count(groups.Sum(g => g.Bills))), C(ReportFormat.Money(groups.Sum(g => g.Net))) },
                Kpis = new() { Kpi("Bills", ReportFormat.Count(sales.Count)), Kpi("Net", ReportFormat.Money(sales.Sum(s => s.TotalAmount))) },
                Notice = "Hours and weekdays use the company time zone."
            };
        }

        var hours = Enumerable.Range(0, 24).Select(h =>
        {
            var match = sales.Where(s => Local(s.SaleDate, input.TimeZone).Hour == h).ToList();
            return new { Hour = h, Bills = match.Count, Net = match.Sum(x => x.TotalAmount) };
        }).ToList();
        return new OutputReportTable
        {
            Columns = new() { Col("Hour"), Col("Bills", "end"), Col("Net", "end") },
            Rows = hours.Select(h => new[]
            {
                C(h.Hour.ToString("00") + ":00"),
                C(ReportFormat.Count(h.Bills)),
                C(ReportFormat.Money(h.Net))
            }).ToList(),
            TotalCount = 24,
            Totals = new[] { C("Totals"), C(ReportFormat.Count(hours.Sum(h => h.Bills))), C(ReportFormat.Money(hours.Sum(h => h.Net))) },
            Kpis = new() { Kpi("Bills", ReportFormat.Count(sales.Count)), Kpi("Net", ReportFormat.Money(sales.Sum(s => s.TotalAmount))) },
            Notice = "The hour is the company local time of the sale."
        };
    }

    private async Task<OutputReportTable> MasterLists(InputReportQuery input, CancellationToken ct)
    {
        var group = (input.GroupBy ?? "customers").Trim().ToLowerInvariant();
        var term = Term(input);
        var (page, size) = Paging(input);

        if (group == "suppliers")
        {
            var q = _db.Suppliers.AsNoTracking().Where(s => !s.Cancelled);
            if (term != null)
                q = q.Where(s => s.SupplierName.ToLower().Contains(term) || (s.SupplierCode != null && s.SupplierCode.ToLower().Contains(term)));
            var total = await q.CountAsync(ct);
            var rows = await q.OrderBy(s => s.SupplierName).Skip((page - 1) * size).Take(size)
                .Select(s => new { s.SupplierCode, s.SupplierName, s.Phone, s.City }).ToListAsync(ct);
            return new OutputReportTable
            {
                Columns = new() { Col("Code"), Col("Supplier"), Col("Phone"), Col("City") },
                Rows = rows.Select(s => new[] { C(s.SupplierCode), C(s.SupplierName), C(s.Phone), C(s.City) }).ToList(),
                TotalCount = total,
                Kpis = new() { Kpi("Suppliers", ReportFormat.Count(total)) }
            };
        }

        if (group == "employees")
        {
            var q = _db.Users.AsNoTracking().Where(u => !u.Cancelled && !u.IsPlatformSuperAdmin);
            if (term != null)
                q = q.Where(u => u.FullName.ToLower().Contains(term) || u.UserName.ToLower().Contains(term) || (u.EmployeeCode != null && u.EmployeeCode.ToLower().Contains(term)));
            var total = await q.CountAsync(ct);
            var rows = await q.OrderBy(u => u.FullName).Skip((page - 1) * size).Take(size)
                .Select(u => new { u.IdUser, u.EmployeeCode, u.FullName, u.UserName }).ToListAsync(ct);
            var ids = rows.Select(r => r.IdUser).ToList();
            var roles = ids.Count == 0
                ? new Dictionary<int, string>()
                : (await (
                    from ur in _db.UserRoles.AsNoTracking()
                    join r in _db.Roles.AsNoTracking() on ur.FkRole equals r.IdRole
                    where ids.Contains(ur.FkUser) && !r.Cancelled
                    select new { ur.FkUser, r.RoleName }).ToListAsync(ct))
                    .GroupBy(x => x.FkUser)
                    .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.RoleName).Distinct()));
            return new OutputReportTable
            {
                Columns = new() { Col("Code"), Col("Name"), Col("Username"), Col("Role") },
                Rows = rows.Select(u => new[] { C(u.EmployeeCode), C(u.FullName), C(u.UserName), C(roles.GetValueOrDefault(u.IdUser)) }).ToList(),
                TotalCount = total,
                Kpis = new() { Kpi("Employees", ReportFormat.Count(total)) },
                Notice = "Platform super admin accounts are excluded."
            };
        }

        if (group == "categories")
        {
            var cats = await _db.Categories.AsNoTracking().Where(c => !c.Cancelled).OrderBy(c => c.CategoryName)
                .Select(c => new { c.IdCategory, c.CategoryName }).ToListAsync(ct);
            var subCounts = await _db.SubCategories.AsNoTracking().Where(s => !s.Cancelled)
                .GroupBy(s => s.FkCategory).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
            var productCounts = await _db.Products.AsNoTracking().Where(p => !p.Cancelled && p.FkCategory != null)
                .GroupBy(p => p.FkCategory!.Value).Select(g => new { g.Key, Count = g.Count() }).ToListAsync(ct);
            var subMap = subCounts.ToDictionary(x => x.Key, x => x.Count);
            var prodMap = productCounts.ToDictionary(x => x.Key, x => x.Count);
            var filtered = term == null ? cats : cats.Where(c => c.CategoryName.ToLower().Contains(term)).ToList();
            return new OutputReportTable
            {
                Columns = new() { Col("Category"), Col("Sub-categories", "end"), Col("Items", "end") },
                Rows = Slice(filtered, page, size).Select(c => new[]
                {
                    Link(c.CategoryName, "/Reports/CategorySales?fkCategory=" + c.IdCategory),
                    C(ReportFormat.Count(subMap.GetValueOrDefault(c.IdCategory))),
                    C(ReportFormat.Count(prodMap.GetValueOrDefault(c.IdCategory)))
                }).ToList(),
                TotalCount = filtered.Count,
                Kpis = new() { Kpi("Categories", ReportFormat.Count(filtered.Count)) }
            };
        }

        var customers = _db.Customers.AsNoTracking().Where(c => !c.Cancelled);
        if (term != null)
            customers = customers.Where(c => c.FullName.ToLower().Contains(term) || (c.Phone != null && c.Phone.Contains(term)) || (c.CustomerCode != null && c.CustomerCode.ToLower().Contains(term)));
        var customerCount = await customers.CountAsync(ct);
        var customerRows = await customers.OrderBy(c => c.FullName).Skip((page - 1) * size).Take(size)
            .Select(c => new { c.CustomerCode, c.FullName, c.Phone, c.Email }).ToListAsync(ct);
        return new OutputReportTable
        {
            Columns = new() { Col("Code"), Col("Customer"), Col("Phone"), Col("Email") },
            Rows = customerRows.Select(c => new[] { C(c.CustomerCode), C(c.FullName), C(c.Phone), C(c.Email) }).ToList(),
            TotalCount = customerCount,
            Kpis = new() { Kpi("Customers", ReportFormat.Count(customerCount)) },
            Notice = "Customer area is not stored, so there is no area-wise list."
        };
    }
}
