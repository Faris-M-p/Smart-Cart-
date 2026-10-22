using Microsoft.EntityFrameworkCore;
using SmartCartPro.Helpers;
using SmartCartPro.Models;

namespace SmartCartPro.Repositories;

public partial class ReportSuiteRepository
{
    private async Task<OutputReportTable> SalesBillWise(InputReportQuery input, CancellationToken ct)
    {
        var q = _db.Sales.AsNoTracking()
            .Where(s => s.SaleDate >= input.FromUtc && s.SaleDate < input.ToUtcExclusive);
        if (input.StoreIds is not null)
            q = q.Where(s => input.StoreIds.Contains(s.FkStore));
        if (input.FkUser is > 0)
            q = q.Where(s => s.CreatedBy == input.FkUser);
        if (input.FkCustomer is > 0)
            q = q.Where(s => s.FkCustomer == input.FkCustomer);

        var mode = (input.PaymentMode ?? string.Empty).Trim().ToLowerInvariant();
        if (mode is "cash" or "card" or "upi" or "wallet" or "bank" or "other")
            q = q.Where(s => _db.Payments.Any(p => p.FkSale == s.IdSale && !p.Cancelled && p.PaymentMethod == mode));

        var status = (input.Status ?? "completed").Trim().ToLowerInvariant();
        var listed = status switch
        {
            "cancelled" => q.Where(s => s.Cancelled || s.Status == "cancelled"),
            "all" => q,
            _ => q.Where(s => !s.Cancelled && s.Status == "completed")
        };
        var completed = q.Where(s => !s.Cancelled && s.Status == "completed");

        var billCount = await completed.CountAsync(ct);
        var gross = billCount == 0 ? 0 : await completed.SumAsync(s => s.SubTotal, ct);
        var discount = billCount == 0 ? 0 : await completed.SumAsync(s => s.DiscountAmount, ct);
        var tax = billCount == 0 ? 0 : await completed.SumAsync(s => s.TaxAmount, ct);
        var net = billCount == 0 ? 0 : await completed.SumAsync(s => s.TotalAmount, ct);
        var totalCount = await listed.CountAsync(ct);
        var (page, size) = Paging(input);
        var desc = Desc(input);
        var sort = (input.SortBy ?? "date").ToLowerInvariant();
        var ordered = sort switch
        {
            "net" => desc ? listed.OrderByDescending(s => s.TotalAmount) : listed.OrderBy(s => s.TotalAmount),
            "invoice" => desc ? listed.OrderByDescending(s => s.InvoiceNumber) : listed.OrderBy(s => s.InvoiceNumber),
            _ => desc ? listed.OrderByDescending(s => s.SaleDate) : listed.OrderBy(s => s.SaleDate)
        };

        var pageRows = await ordered.Skip((page - 1) * size).Take(size)
            .Select(s => new
            {
                s.IdSale,
                s.InvoiceNumber,
                s.SaleDate,
                s.FkCustomer,
                s.CreatedBy,
                s.SubTotal,
                s.DiscountAmount,
                s.TaxAmount,
                s.TotalAmount,
                s.Status,
                s.Cancelled
            }).ToListAsync(ct);

        var ids = pageRows.Select(r => r.IdSale).ToList();
        var countMap = ids.Count == 0
            ? new Dictionary<int, int>()
            : (await _db.SaleDetails.AsNoTracking().Where(d => ids.Contains(d.FkSale))
                .GroupBy(d => d.FkSale)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync(ct)).ToDictionary(x => x.Key, x => x.Count);

        var payMap = ids.Count == 0
            ? new Dictionary<int, string>()
            : (await _db.Payments.AsNoTracking()
                .Where(p => p.FkSale != null && ids.Contains(p.FkSale.Value) && !p.Cancelled)
                .Select(p => new { SaleId = p.FkSale!.Value, p.PaymentMethod })
                .ToListAsync(ct))
                .GroupBy(p => p.SaleId)
                .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.PaymentMethod).Distinct()));

        var customerIds = pageRows.Where(r => r.FkCustomer != null).Select(r => r.FkCustomer!.Value).Distinct().ToList();
        var custMap = customerIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Customers.AsNoTracking().Where(c => customerIds.Contains(c.IdCustomer))
                .ToDictionaryAsync(c => c.IdCustomer, c => c.FullName, ct);

        var userIds = pageRows.Where(r => r.CreatedBy != null).Select(r => r.CreatedBy!.Value).Distinct().ToList();
        var userMap = userIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Users.AsNoTracking().Where(u => userIds.Contains(u.IdUser))
                .ToDictionaryAsync(u => u.IdUser, u => u.FullName, ct);

        var rows = new List<ReportCell[]>();
        foreach (var s in pageRows)
        {
            var muted = s.Cancelled || string.Equals(s.Status, "cancelled", StringComparison.OrdinalIgnoreCase);
            rows.Add(new[]
            {
                Link(s.InvoiceNumber, "/Sales/InvoiceDetails/" + s.IdSale, muted),
                C(ReportFormat.When(Local(s.SaleDate, input.TimeZone)), muted),
                C(s.FkCustomer is int cid && custMap.TryGetValue(cid, out var cn) ? cn : "Walk-in", muted),
                Link(s.CreatedBy is int uid && userMap.TryGetValue(uid, out var un) ? un : "—",
                    "/Reports/DayClose?fkUser=" + (s.CreatedBy ?? 0) + "&" + Dates(input) + StoreQ(input), muted),
                C(ReportFormat.Count(countMap.GetValueOrDefault(s.IdSale)), muted),
                C(ReportFormat.Money(s.SubTotal), muted),
                C(ReportFormat.Money(s.DiscountAmount), muted),
                C(ReportFormat.Money(s.TaxAmount), muted),
                C(ReportFormat.Money(s.TotalAmount), muted),
                C(payMap.GetValueOrDefault(s.IdSale), muted),
                C(muted ? "Cancelled" : "Completed", muted)
            });
        }

        return new OutputReportTable
        {
            Columns = new()
            {
                Col("Bill no.", sort: "invoice"), Col("Date / time", sort: "date"), Col("Customer"), Col("Cashier"),
                Col("Items", "end"), Col("Gross", "end"), Col("Discount", "end"), Col("Tax", "end"),
                Col("Net", "end", "net"), Col("Payment"), Col("Status")
            },
            Rows = rows,
            TotalCount = totalCount,
            Totals = new[]
            {
                C("Totals"), C(""), C(""), C(""), C(ReportFormat.Count(billCount)),
                C(ReportFormat.Money(gross)), C(ReportFormat.Money(discount)), C(ReportFormat.Money(tax)),
                C(ReportFormat.Money(net)), C(""), C("")
            },
            Kpis = new()
            {
                Kpi("Bills", ReportFormat.Count(billCount)),
                Kpi("Gross", ReportFormat.Money(gross)),
                Kpi("Discount", ReportFormat.Money(discount)),
                Kpi("Tax", ReportFormat.Money(tax)),
                Kpi("Net", ReportFormat.Money(net))
            },
            Notice = "Round-off is not stored. Gross is the line amount before discount. Cancelled bills are grey and left out of the totals."
        };
    }

    private async Task<OutputReportTable> ItemSales(InputReportQuery input, CancellationToken ct)
    {
        var sold = await LoadSoldAsync(input, ct);
        var returns = await LoadReturnsAsync(input, ct);
        var keys = sold.Select(s => (Product: s.ProductId, Variant: s.VariantId)).Union(returns.Keys).Distinct().ToList();
        var labels = await SkuLabelsAsync(keys, ct);

        var lines = keys.Select(k =>
        {
            var s = sold.FirstOrDefault(x => x.ProductId == k.Product && x.VariantId == k.Variant);
            returns.TryGetValue(k, out var ret);
            labels.TryGetValue(k, out var label);
            return new
            {
                ProductId = k.Product,
                VariantId = k.Variant,
                Name = label.Name,
                Sku = label.Sku,
                Barcode = label.Barcode,
                Unit = label.Unit,
                Qty = s?.Qty ?? 0,
                ReturnQty = ret.Qty,
                Discount = s?.Discount ?? 0,
                Tax = s?.Tax ?? 0,
                Gross = s?.Gross ?? 0,
                Net = s?.Net ?? 0
            };
        }).ToList();

        var desc = Desc(input);
        var sort = (input.SortBy ?? "net").ToLowerInvariant();
        lines = sort switch
        {
            "name" => desc ? lines.OrderByDescending(x => x.Name).ToList() : lines.OrderBy(x => x.Name).ToList(),
            "qty" => desc ? lines.OrderByDescending(x => x.Qty).ToList() : lines.OrderBy(x => x.Qty).ToList(),
            _ => desc ? lines.OrderByDescending(x => x.Net).ToList() : lines.OrderBy(x => x.Net).ToList()
        };

        var (page, size) = Paging(input);
        var rows = Slice(lines, page, size).Select(x => new[]
        {
            Link(x.Name, "/Reports/ItemLedger?fkProduct=" + x.ProductId + (x.VariantId > 0 ? "&fkProductVariant=" + x.VariantId : "") + "&" + Dates(input) + StoreQ(input)),
            C(x.Sku), C(x.Barcode), C(x.Unit),
            C(ReportFormat.Qty(x.Qty)), C(ReportFormat.Qty(x.ReturnQty)), C(ReportFormat.Qty(x.Qty - x.ReturnQty)),
            C(ReportFormat.Money(x.Gross)), C(ReportFormat.Money(x.Discount)), C(ReportFormat.Money(x.Tax)), C(ReportFormat.Money(x.Net))
        }).ToList();

        return new OutputReportTable
        {
            Columns = new()
            {
                Col("Item", sort: "name"), Col("SKU"), Col("Barcode"), Col("Unit"),
                Col("Qty sold", "end", "qty"), Col("Return qty", "end"), Col("Net qty", "end"),
                Col("Sales value", "end"), Col("Discount", "end"), Col("Tax", "end"), Col("Net value", "end", "net")
            },
            Rows = rows,
            TotalCount = lines.Count,
            Totals = new[]
            {
                C("Totals"), C(""), C(""), C(""),
                C(ReportFormat.Qty(lines.Sum(x => x.Qty))),
                C(ReportFormat.Qty(lines.Sum(x => x.ReturnQty))),
                C(ReportFormat.Qty(lines.Sum(x => x.Qty - x.ReturnQty))),
                C(ReportFormat.Money(lines.Sum(x => x.Gross))),
                C(ReportFormat.Money(lines.Sum(x => x.Discount))),
                C(ReportFormat.Money(lines.Sum(x => x.Tax))),
                C(ReportFormat.Money(lines.Sum(x => x.Net)))
            },
            Kpis = new()
            {
                Kpi("Items", ReportFormat.Count(lines.Count)),
                Kpi("Qty sold", ReportFormat.Qty(lines.Sum(x => x.Qty))),
                Kpi("Net value", ReportFormat.Money(lines.Sum(x => x.Net)))
            },
            Notice = "Net value is the sale line total and ties to the bill-wise net. Return quantity is shown beside it and is not deducted from net value."
        };
    }

    private async Task<OutputReportTable> CategorySales(InputReportQuery input, CancellationToken ct)
    {
        var bySub = string.Equals(input.Level, "subcategory", StringComparison.OrdinalIgnoreCase);
        var rowsQ =
            from d in _db.SaleDetails.AsNoTracking()
            join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
            join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
            where !s.Cancelled && s.Status == "completed"
                && s.SaleDate >= input.FromUtc && s.SaleDate < input.ToUtcExclusive
                && (input.StoreIds == null || input.StoreIds.Contains(s.FkStore))
            group new { s.IdSale, d.Quantity, d.LineTotal } by new
            {
                Category = p.FkCategory ?? 0,
                Sub = bySub ? p.FkSubcategory ?? 0 : 0
            }
            into g
            select new
            {
                g.Key.Category,
                g.Key.Sub,
                Bills = g.Select(x => x.IdSale).Distinct().Count(),
                Qty = g.Sum(x => x.Quantity),
                Net = g.Sum(x => x.LineTotal)
            };

        var facts = await rowsQ.ToListAsync(ct);
        var totalNet = facts.Sum(x => x.Net);
        var catIds = facts.Select(x => x.Category).Where(id => id > 0).Distinct().ToList();
        var subIds = facts.Select(x => x.Sub).Where(id => id > 0).Distinct().ToList();
        var cats = catIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Categories.AsNoTracking().Where(c => catIds.Contains(c.IdCategory)).ToDictionaryAsync(c => c.IdCategory, c => c.CategoryName, ct);
        var subs = subIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.SubCategories.AsNoTracking().Where(s => subIds.Contains(s.IdSubcategory)).ToDictionaryAsync(s => s.IdSubcategory, s => s.SubCategoryName, ct);

        var desc = Desc(input);
        var ordered = (desc ? facts.OrderByDescending(x => x.Net) : facts.OrderBy(x => x.Net)).ToList();
        var (page, size) = Paging(input);
        var pageRows = Slice(ordered, page, size).Select(x =>
        {
            var share = totalNet == 0 ? 0 : Math.Round(x.Net * 100m / totalNet, 2, MidpointRounding.AwayFromZero);
            var href = "/Reports/ItemSales?fkCategory=" + x.Category + "&" + Dates(input) + StoreQ(input);
            return new[]
            {
                Link(x.Category > 0 && cats.TryGetValue(x.Category, out var cn) ? cn : "Uncategorised", href),
                C(bySub ? (x.Sub > 0 && subs.TryGetValue(x.Sub, out var sn) ? sn : "—") : "—"),
                C(ReportFormat.Count(x.Bills)),
                C(ReportFormat.Qty(x.Qty)),
                C(ReportFormat.Money(x.Net)),
                C(ReportFormat.Pct(share))
            };
        }).ToList();

        return new OutputReportTable
        {
            Columns = new()
            {
                Col("Category"), Col("Sub-category"), Col("Bills", "end"), Col("Qty", "end"),
                Col("Net value", "end", "net"), Col("% of total", "end")
            },
            Rows = pageRows,
            TotalCount = ordered.Count,
            Totals = new[]
            {
                C("Totals"), C(""), C(ReportFormat.Count(facts.Sum(x => x.Bills))),
                C(ReportFormat.Qty(facts.Sum(x => x.Qty))), C(ReportFormat.Money(totalNet)), C("100%")
            },
            Kpis = new()
            {
                Kpi("Groups", ReportFormat.Count(facts.Count)),
                Kpi("Net value", ReportFormat.Money(totalNet))
            },
            Notice = "Net value is the sale line total, the same basis as item-wise sales. A bill in two categories is counted in each."
        };
    }

    private async Task<OutputReportTable> SalesReturns(InputReportQuery input, CancellationToken ct)
    {
        var q = _db.SalesReturns.AsNoTracking()
            .Where(r => !r.Cancelled && r.ReturnDate >= input.FromUtc && r.ReturnDate < input.ToUtcExclusive);
        if (input.StoreIds is not null)
            q = q.Where(r => input.StoreIds.Contains(r.FkStore));
        if (input.FkUser is > 0)
            q = q.Where(r => r.CreatedBy == input.FkUser);
        if (input.FkCustomer is > 0)
            q = q.Where(r => _db.Sales.Any(s => s.IdSale == r.FkSale && s.FkCustomer == input.FkCustomer));

        var totalCount = await q.CountAsync(ct);
        var qty = totalCount == 0 ? 0 : await (
            from d in _db.SalesReturnDetails.AsNoTracking()
            join r in q on d.FkSalesReturn equals r.IdSalesReturn
            select d.Quantity).SumAsync(ct);
        var amount = totalCount == 0 ? 0 : await q.SumAsync(r => r.TotalAmount, ct);
        var (page, size) = Paging(input);
        var desc = Desc(input);
        var ordered = desc ? q.OrderByDescending(r => r.ReturnDate) : q.OrderBy(r => r.ReturnDate);
        var pageRows = await ordered.Skip((page - 1) * size).Take(size)
            .Select(r => new { r.IdSalesReturn, r.ReturnNumber, r.ReturnDate, r.FkSale, r.TotalAmount, r.Reason, r.CreatedBy })
            .ToListAsync(ct);

        var saleIds = pageRows.Select(r => r.FkSale).Distinct().ToList();
        var sales = saleIds.Count == 0
            ? new Dictionary<int, (string Invoice, int? Customer)>()
            : (await _db.Sales.AsNoTracking().Where(s => saleIds.Contains(s.IdSale))
                .Select(s => new { s.IdSale, s.InvoiceNumber, s.FkCustomer }).ToListAsync(ct))
                .ToDictionary(s => s.IdSale, s => (Invoice: s.InvoiceNumber, Customer: s.FkCustomer));
        var customerIds = sales.Values.Where(s => s.Customer != null).Select(s => s.Customer!.Value).Distinct().ToList();
        var custMap = customerIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Customers.AsNoTracking().Where(c => customerIds.Contains(c.IdCustomer)).ToDictionaryAsync(c => c.IdCustomer, c => c.FullName, ct);
        var userIds = pageRows.Where(r => r.CreatedBy != null).Select(r => r.CreatedBy!.Value).Distinct().ToList();
        var userMap = userIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Users.AsNoTracking().Where(u => userIds.Contains(u.IdUser)).ToDictionaryAsync(u => u.IdUser, u => u.FullName, ct);
        var returnIds = pageRows.Select(r => r.IdSalesReturn).ToList();
        var counts = returnIds.Count == 0
            ? new Dictionary<int, (int Lines, decimal Qty)>()
            : (await _db.SalesReturnDetails.AsNoTracking().Where(d => returnIds.Contains(d.FkSalesReturn))
                .GroupBy(d => d.FkSalesReturn)
                .Select(g => new { g.Key, Lines = g.Count(), Qty = g.Sum(x => x.Quantity) })
                .ToListAsync(ct)).ToDictionary(x => x.Key, x => (x.Lines, x.Qty));

        var rows = pageRows.Select(r =>
        {
            sales.TryGetValue(r.FkSale, out var sale);
            var customer = sale.Customer is int cid && custMap.TryGetValue(cid, out var name) ? name : "Walk-in";
            counts.TryGetValue(r.IdSalesReturn, out var line);
            return new[]
            {
                Link(r.ReturnNumber, "/Sales/ReturnDetails/" + r.IdSalesReturn),
                C(ReportFormat.When(Local(r.ReturnDate, input.TimeZone))),
                Link(sale.Invoice, "/Sales/InvoiceDetails/" + r.FkSale),
                C(customer),
                C(ReportFormat.Count(line.Lines)),
                C(ReportFormat.Qty(line.Qty)),
                C(ReportFormat.Money(r.TotalAmount)),
                C(r.Reason),
                C(r.CreatedBy is int uid && userMap.TryGetValue(uid, out var un) ? un : "—")
            };
        }).ToList();

        return new OutputReportTable
        {
            Columns = new()
            {
                Col("Return no."), Col("Date", sort: "date"), Col("Original bill"), Col("Customer"),
                Col("Items", "end"), Col("Qty", "end"), Col("Refund amount", "end"), Col("Reason"), Col("By")
            },
            Rows = rows,
            TotalCount = totalCount,
            Totals = new[]
            {
                C("Totals"), C(""), C(""), C(""), C(""), C(ReportFormat.Qty(qty)), C(ReportFormat.Money(amount)), C(""), C("")
            },
            Kpis = new()
            {
                Kpi("Returns", ReportFormat.Count(totalCount)),
                Kpi("Qty", ReportFormat.Qty(qty)),
                Kpi("Refund", ReportFormat.Money(amount))
            },
            Notice = "Refund mode is not stored on a return, so that column is omitted."
        };
    }

    private async Task<OutputReportTable> DayClose(InputReportQuery input, CancellationToken ct)
    {
        var q = _db.CashSessions.AsNoTracking()
            .Where(s => s.OpenedAt >= input.FromUtc && s.OpenedAt < input.ToUtcExclusive);
        if (input.StoreIds is not null)
            q = q.Where(s => input.StoreIds.Contains(s.FkStore));
        if (input.FkUser is > 0)
            q = q.Where(s => s.FkUser == input.FkUser);

        var sessions = await q.OrderByDescending(s => s.OpenedAt)
            .Select(s => new
            {
                s.IdCashSession, s.FkUser, s.OpenedAt, s.ClosedAt, s.OpeningFloat,
                s.ClosingCash, s.ExpectedCash, s.Variance, s.Status
            }).ToListAsync(ct);

        var ids = sessions.Select(s => s.IdCashSession).ToList();
        var pays = ids.Count == 0
            ? new List<(int Session, string Method, decimal Amount)>()
            : (await _db.Payments.AsNoTracking()
                .Where(p => p.FkCashSession != null && ids.Contains(p.FkCashSession.Value) && !p.Cancelled)
                .GroupBy(p => new { Session = p.FkCashSession!.Value, p.PaymentMethod })
                .Select(g => new { g.Key.Session, g.Key.PaymentMethod, Amount = g.Sum(x => x.Amount) })
                .ToListAsync(ct)).Select(x => (Session: x.Session, Method: x.PaymentMethod, Amount: x.Amount)).ToList();

        var saleIds = ids.Count == 0
            ? new List<int>()
            : await _db.Sales.AsNoTracking().Where(s => s.FkCashSession != null && ids.Contains(s.FkCashSession.Value))
                .Select(s => s.IdSale).ToListAsync(ct);
        var returnBySale = saleIds.Count == 0
            ? new Dictionary<int, decimal>()
            : (await _db.SalesReturns.AsNoTracking()
                .Where(r => !r.Cancelled && saleIds.Contains(r.FkSale))
                .GroupBy(r => r.FkSale)
                .Select(g => new { g.Key, Amount = g.Sum(x => x.TotalAmount) })
                .ToListAsync(ct)).ToDictionary(x => x.Key, x => x.Amount);
        var sessionOfSale = saleIds.Count == 0
            ? new Dictionary<int, int>()
            : (await _db.Sales.AsNoTracking().Where(s => saleIds.Contains(s.IdSale))
                .Select(s => new { s.IdSale, Session = s.FkCashSession ?? 0 }).ToListAsync(ct))
                .ToDictionary(s => s.IdSale, s => s.Session);

        var userIds = sessions.Select(s => s.FkUser).Distinct().ToList();
        var users = userIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Users.AsNoTracking().Where(u => userIds.Contains(u.IdUser)).ToDictionaryAsync(u => u.IdUser, u => u.FullName, ct);

        decimal SumMethod(int session, params string[] methods) =>
            pays.Where(p => p.Session == session && methods.Contains(p.Method)).Sum(p => p.Amount);

        var built = sessions.Select(s =>
        {
            var cash = SumMethod(s.IdCashSession, "cash");
            var card = SumMethod(s.IdCashSession, "card");
            var upi = SumMethod(s.IdCashSession, "upi");
            var other = pays.Where(p => p.Session == s.IdCashSession && p.Method is not ("cash" or "card" or "upi")).Sum(p => p.Amount);
            var refunds = returnBySale.Where(r => sessionOfSale.TryGetValue(r.Key, out var sid) && sid == s.IdCashSession).Sum(r => r.Value);
            var expected = s.OpeningFloat + cash;
            var counted = s.ClosingCash;
            var diff = counted is decimal c ? c - expected : s.Variance;
            return new { s, cash, card, upi, other, refunds, expected, counted, diff, name = users.GetValueOrDefault(s.FkUser) ?? "—" };
        }).ToList();

        var (page, size) = Paging(input);
        var rows = Slice(built, page, size).Select(x => new[]
        {
            Link(x.name, "/Reports/DayClose?fkUser=" + x.s.FkUser + "&" + Dates(input) + StoreQ(input)),
            C(ReportFormat.When(Local(x.s.OpenedAt, input.TimeZone))),
            C(x.s.ClosedAt is DateTime closed ? ReportFormat.When(Local(closed, input.TimeZone)) : "Open"),
            C(ReportFormat.Money(x.s.OpeningFloat)),
            C(ReportFormat.Money(x.cash)),
            C(ReportFormat.Money(x.card)),
            C(ReportFormat.Money(x.upi)),
            C(ReportFormat.Money(x.other)),
            C(ReportFormat.Money(x.refunds)),
            C(ReportFormat.Money(x.expected)),
            C(x.counted is decimal c ? ReportFormat.Money(c) : "—"),
            C(x.diff is decimal d ? ReportFormat.Money(d) : "—", warn: x.diff is decimal w && w != 0)
        }).ToList();

        return new OutputReportTable
        {
            Columns = new()
            {
                Col("Cashier"), Col("Opened"), Col("Closed"), Col("Opening", "end"), Col("Cash sales", "end"),
                Col("Card", "end"), Col("UPI", "end"), Col("Other", "end"), Col("Returns", "end"),
                Col("Expected cash", "end"), Col("Counted cash", "end"), Col("Difference", "end")
            },
            Rows = rows,
            TotalCount = built.Count,
            Totals = new[]
            {
                C("Totals"), C(""), C(""),
                C(ReportFormat.Money(built.Sum(x => x.s.OpeningFloat))),
                C(ReportFormat.Money(built.Sum(x => x.cash))),
                C(ReportFormat.Money(built.Sum(x => x.card))),
                C(ReportFormat.Money(built.Sum(x => x.upi))),
                C(ReportFormat.Money(built.Sum(x => x.other))),
                C(ReportFormat.Money(built.Sum(x => x.refunds))),
                C(ReportFormat.Money(built.Sum(x => x.expected))),
                C(ReportFormat.Money(built.Where(x => x.counted != null).Sum(x => x.counted ?? 0))),
                C("")
            },
            Kpis = new()
            {
                Kpi("Sessions", ReportFormat.Count(built.Count)),
                Kpi("Cash", ReportFormat.Money(built.Sum(x => x.cash))),
                Kpi("Card", ReportFormat.Money(built.Sum(x => x.card))),
                Kpi("UPI", ReportFormat.Money(built.Sum(x => x.upi)))
            },
            Notice = "Expected cash is opening float plus cash payments. Cash refunds are not stored separately, so returns are the session return total and are not subtracted from expected cash."
        };
    }

    private async Task<Dictionary<(int Product, int Variant), (string Name, string? Sku, string? Barcode, string Unit)>> SkuLabelsAsync(
        IReadOnlyCollection<(int Product, int Variant)> keys, CancellationToken ct)
    {
        var productIds = keys.Select(k => k.Product).Distinct().ToList();
        var variantIds = keys.Where(k => k.Variant > 0).Select(k => k.Variant).Distinct().ToList();
        var products = productIds.Count == 0
            ? new List<(int Id, string Name, string? Code, string? Sku, string? Barcode, int? Unit)>()
            : (await _db.Products.AsNoTracking().Where(p => productIds.Contains(p.IdProduct))
                .Select(p => new { p.IdProduct, p.ProductName, p.ProductCode, p.Sku, p.Barcode, p.FkUnit })
                .ToListAsync(ct)).Select(p => (Id: p.IdProduct, Name: p.ProductName, Code: p.ProductCode, Sku: p.Sku, Barcode: p.Barcode, Unit: p.FkUnit)).ToList();
        var variants = variantIds.Count == 0
            ? new Dictionary<int, (string Name, string? Sku, string? Barcode)>()
            : (await _db.ProductVariants.AsNoTracking().Where(v => variantIds.Contains(v.IdProductVariant))
                .Select(v => new { v.IdProductVariant, v.VariantName, v.SkuCode, v.Barcode }).ToListAsync(ct))
                .ToDictionary(v => v.IdProductVariant, v => (Name: v.VariantName, Sku: v.SkuCode, Barcode: v.Barcode));
        var unitIds = products.Where(p => p.Unit != null).Select(p => p.Unit!.Value).Distinct().ToList();
        var units = unitIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Units.AsNoTracking().Where(u => unitIds.Contains(u.IdUnit)).ToDictionaryAsync(u => u.IdUnit, u => u.UnitName, ct);
        var productMap = products.ToDictionary(p => p.Id);

        var map = new Dictionary<(int Product, int Variant), (string Name, string? Sku, string? Barcode, string Unit)>();
        foreach (var key in keys.Distinct())
        {
            if (!productMap.TryGetValue(key.Product, out var p))
            {
                map[key] = ("Item " + key.Product, null, null, "—");
                continue;
            }

            variants.TryGetValue(key.Variant, out var v);
            var name = key.Variant > 0 && !string.IsNullOrWhiteSpace(v.Name) ? p.Name + " / " + v.Name : p.Name;
            var sku = key.Variant > 0 && !string.IsNullOrWhiteSpace(v.Sku) ? v.Sku : (p.Sku ?? p.Code);
            var barcode = key.Variant > 0 && !string.IsNullOrWhiteSpace(v.Barcode) ? v.Barcode : p.Barcode;
            var unit = p.Unit is int uid && units.TryGetValue(uid, out var un) ? un : "—";
            map[key] = (name, sku, barcode, unit);
        }

        return map;
    }
}
