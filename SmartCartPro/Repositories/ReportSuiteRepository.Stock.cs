using Microsoft.EntityFrameworkCore;
using SmartCartPro.Helpers;
using SmartCartPro.Models;

namespace SmartCartPro.Repositories;

public partial class ReportSuiteRepository
{
    private async Task<OutputReportTable> CurrentStock(InputReportQuery input, CancellationToken ct)
    {
        var term = Term(input);
        var q =
            from i in _db.Inventories.AsNoTracking()
            join p in _db.Products.AsNoTracking() on i.FkProduct equals p.IdProduct
            join st in _db.Stores.AsNoTracking() on i.FkStore equals st.IdStore
            join v in _db.ProductVariants.AsNoTracking() on i.FkProductVariant equals (int?)v.IdProductVariant into vj
            from v in vj.DefaultIfEmpty()
            join u in _db.Units.AsNoTracking() on p.FkUnit equals (int?)u.IdUnit into uj
            from u in uj.DefaultIfEmpty()
            where !p.Cancelled
                && (input.StoreIds == null || input.StoreIds.Contains(i.FkStore))
                && (!(input.FkCategory > 0) || p.FkCategory == input.FkCategory)
                && (!(input.FkSubCategory > 0) || p.FkSubcategory == input.FkSubCategory)
                && (!(input.FkBrand > 0) || p.FkBrand == input.FkBrand)
                && (term == null
                    || p.ProductName.ToLower().Contains(term)
                    || (p.Sku != null && p.Sku.ToLower().Contains(term))
                    || (p.ProductCode != null && p.ProductCode.ToLower().Contains(term))
                    || (v != null && v.VariantName.ToLower().Contains(term)))
                && (input.OnlyNegative || input.ShowZero || i.QuantityOnHand != 0)
                && (!input.OnlyNegative || i.QuantityOnHand < 0)
            select new
            {
                i.FkProduct,
                Variant = i.FkProductVariant ?? 0,
                Name = v == null ? p.ProductName : p.ProductName + " / " + v.VariantName,
                Sku = v != null && v.SkuCode != null ? v.SkuCode : (p.Sku ?? p.ProductCode),
                Unit = u == null ? "—" : u.UnitName,
                Store = st.StoreName,
                Qty = i.QuantityOnHand,
                Reorder = i.ReorderLevel > 0 ? i.ReorderLevel : p.ReorderLevel,
                Cost = p.CostPrice,
                Selling = p.SellingPrice + (v == null ? 0 : v.AdditionalPrice)
            };

        var all = await q.ToListAsync(ct);
        var desc = Desc(input);
        var sort = (input.SortBy ?? "name").ToLowerInvariant();
        all = sort switch
        {
            "qty" => desc ? all.OrderByDescending(x => x.Qty).ToList() : all.OrderBy(x => x.Qty).ToList(),
            _ => desc ? all.OrderByDescending(x => x.Name).ToList() : all.OrderBy(x => x.Name).ToList()
        };

        var showCost = input.CanSeeProfit;
        var (page, size) = Paging(input);
        var rows = Slice(all, page, size).Select(x =>
        {
            var cells = new List<ReportCell>
            {
                Link(x.Name, "/Reports/ItemLedger?fkProduct=" + x.FkProduct + (x.Variant > 0 ? "&fkProductVariant=" + x.Variant : "") + StoreQ(input)),
                C(x.Sku), C(x.Unit), C(x.Store), C(ReportFormat.Qty(x.Qty)), C(ReportFormat.Qty(x.Reorder))
            };
            if (showCost)
            {
                cells.Add(C(ReportFormat.Money(x.Cost)));
                cells.Add(C(ReportFormat.Money(x.Qty * x.Cost)));
            }
            cells.Add(C(ReportFormat.Money(x.Selling)));
            return cells.ToArray();
        }).ToList();

        var columns = new List<ReportColumn>
        {
            Col("Item", sort: "name"), Col("SKU"), Col("Unit"), Col("Store"),
            Col("Qty", "end", "qty"), Col("Reorder", "end")
        };
        if (showCost)
        {
            columns.Add(Col("Cost", "end"));
            columns.Add(Col("Stock value", "end"));
        }
        columns.Add(Col("Selling price", "end"));

        var totals = new List<ReportCell>
        {
            C("Totals"), C(""), C(""), C(ReportFormat.Count(all.Count) + " items"),
            C(ReportFormat.Qty(all.Sum(x => x.Qty))), C("")
        };
        if (showCost)
        {
            totals.Add(C(""));
            totals.Add(C(ReportFormat.Money(all.Sum(x => x.Qty * x.Cost))));
        }
        totals.Add(C(""));

        var kpis = new List<OutputReportKpi>
        {
            Kpi("Items", ReportFormat.Count(all.Count)),
            Kpi("Qty", ReportFormat.Qty(all.Sum(x => x.Qty)))
        };
        if (showCost)
            kpis.Add(Kpi("Stock value", ReportFormat.Money(all.Sum(x => x.Qty * x.Cost))));

        return new OutputReportTable
        {
            Columns = columns,
            Rows = rows,
            TotalCount = all.Count,
            Totals = totals.ToArray(),
            Kpis = kpis,
            Notice = input.OnlyNegative
                ? "On-hand quantity is stored with a check that it cannot go below zero, so this filter is empty unless that check is removed later."
                : showCost ? null : "Cost and stock value are hidden without the reports.profit permission."
        };
    }

    private async Task<OutputReportTable> ItemLedger(InputReportQuery input, CancellationToken ct)
    {
        var productId = input.FkProduct ?? 0;
        var variantId = input.FkProductVariant ?? 0;
        if (productId <= 0)
        {
            var term = Term(input);
            if (term is null)
            {
                return new OutputReportTable
                {
                    Notice = "Search for an item, or open this report from a stock or sales row.",
                    Columns = new() { Col("Item"), Col("SKU"), Col("Code") }
                };
            }

            var matches = await (
                from p in _db.Products.AsNoTracking()
                join v in _db.ProductVariants.AsNoTracking() on p.IdProduct equals v.FkProduct into vj
                from v in vj.DefaultIfEmpty()
                where !p.Cancelled && (v == null || !v.Cancelled)
                    && (p.ProductName.ToLower().Contains(term)
                        || (p.Sku != null && p.Sku.ToLower().Contains(term))
                        || (p.ProductCode != null && p.ProductCode.ToLower().Contains(term))
                        || (v != null && v.VariantName.ToLower().Contains(term))
                        || (v != null && v.SkuCode != null && v.SkuCode.ToLower().Contains(term)))
                orderby p.ProductName
                select new
                {
                    p.IdProduct,
                    Variant = v == null ? 0 : v.IdProductVariant,
                    Name = v == null ? p.ProductName : p.ProductName + " / " + v.VariantName,
                    Sku = v != null && v.SkuCode != null ? v.SkuCode : p.Sku,
                    Code = p.ProductCode
                }).Take(50).ToListAsync(ct);

            if (matches.Count == 1)
            {
                productId = matches[0].IdProduct;
                variantId = matches[0].Variant;
            }
            else
            {
                return new OutputReportTable
                {
                    Notice = matches.Count == 0 ? "No item matches that search." : "Choose an item.",
                    Columns = new() { Col("Item"), Col("SKU"), Col("Code") },
                    Rows = matches.Select(m => new[]
                    {
                        Link(m.Name, "/Reports/ItemLedger?fkProduct=" + m.IdProduct + (m.Variant > 0 ? "&fkProductVariant=" + m.Variant : "") + "&" + Dates(input) + StoreQ(input)),
                        C(m.Sku), C(m.Code)
                    }).ToList(),
                    TotalCount = matches.Count
                };
            }
        }

        var moves = _db.StockMovements.AsNoTracking()
            .Where(m => m.FkProduct == productId && (m.FkProductVariant ?? 0) == variantId);
        if (input.StoreIds is not null)
            moves = moves.Where(m => input.StoreIds.Contains(m.FkStore));

        var opening = await moves.Where(m => m.CreatedAt < input.FromUtc)
            .SumAsync(m => m.MovementType == "sale" || m.MovementType == "transfer_out" || m.MovementType == "return_out"
                ? -m.Quantity : m.Quantity, ct);

        var inRange = await moves.Where(m => m.CreatedAt >= input.FromUtc && m.CreatedAt < input.ToUtcExclusive)
            .OrderBy(m => m.CreatedAt).ThenBy(m => m.IdStockMovement)
            .Select(m => new
            {
                m.CreatedAt, m.MovementType, m.Quantity, m.ReferenceType, m.ReferenceId, m.Notes, m.CreatedBy, m.FkStore
            }).ToListAsync(ct);

        var userIds = inRange.Where(m => m.CreatedBy != null).Select(m => m.CreatedBy!.Value).Distinct().ToList();
        var users = userIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Users.AsNoTracking().Where(u => userIds.Contains(u.IdUser)).ToDictionaryAsync(u => u.IdUser, u => u.FullName, ct);

        var docs = await DocumentLabelsAsync(inRange.Select(m => (m.ReferenceType, m.ReferenceId)).ToList(), ct);
        decimal balance = opening;
        var built = new List<(DateTime At, string Type, string Doc, string? Href, string Party, decimal InQty, decimal OutQty, decimal Balance, string User)>();
        foreach (var m in inRange)
        {
            var signed = SignedQty(m.MovementType, m.Quantity);
            var inQty = signed > 0 ? signed : 0;
            var outQty = signed < 0 ? -signed : 0;
            balance += signed;
            docs.TryGetValue((m.ReferenceType ?? "", m.ReferenceId ?? 0), out var doc);
            built.Add((m.CreatedAt, m.MovementType, doc.Label ?? m.Notes ?? m.MovementType, doc.Href, doc.Party ?? "—", inQty, outQty, balance,
                m.CreatedBy is int uid && users.TryGetValue(uid, out var un) ? un : "—"));
        }

        var closing = balance;
        var onHand = await _db.Inventories.AsNoTracking()
            .Where(i => i.FkProduct == productId && (i.FkProductVariant ?? 0) == variantId
                && (input.StoreIds == null || input.StoreIds.Contains(i.FkStore)))
            .SumAsync(i => (decimal?)i.QuantityOnHand, ct) ?? 0;
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, input.TimeZone));
        string? notice = null;
        if (input.ToLocal >= today && onHand != closing)
            notice = "Closing quantity " + ReportFormat.Qty(closing) + " does not match current on-hand " + ReportFormat.Qty(onHand) + ". Movements after the end date, or stock written without a movement, would cause this.";

        var (page, size) = Paging(input);
        var rows = Slice(built, page, size).Select(x => new[]
        {
            C(ReportFormat.When(Local(x.At, input.TimeZone))),
            C(x.Type),
            x.Href is null ? C(x.Doc) : Link(x.Doc, x.Href),
            C(x.Party),
            C(ReportFormat.Qty(x.InQty)),
            C(ReportFormat.Qty(x.OutQty)),
            C(ReportFormat.Qty(x.Balance)),
            C(x.User)
        }).ToList();

        return new OutputReportTable
        {
            Columns = new()
            {
                Col("Date / time"), Col("Movement"), Col("Document"), Col("Party"),
                Col("In", "end"), Col("Out", "end"), Col("Balance", "end"), Col("User")
            },
            Rows = rows,
            TotalCount = built.Count,
            Totals = new[]
            {
                C("Closing"), C(""), C(""), C(""),
                C(ReportFormat.Qty(built.Sum(x => x.InQty))),
                C(ReportFormat.Qty(built.Sum(x => x.OutQty))),
                C(ReportFormat.Qty(closing)), C("")
            },
            Kpis = new()
            {
                Kpi("Opening", ReportFormat.Qty(opening)),
                Kpi("In", ReportFormat.Qty(built.Sum(x => x.InQty))),
                Kpi("Out", ReportFormat.Qty(built.Sum(x => x.OutQty))),
                Kpi("Closing", ReportFormat.Qty(closing))
            },
            Notice = notice
        };
    }

    private async Task<OutputReportTable> StockLedger(InputReportQuery input, CancellationToken ct)
    {
        var term = Term(input);
        var moves =
            from m in _db.StockMovements.AsNoTracking()
            join p in _db.Products.AsNoTracking() on m.FkProduct equals p.IdProduct
            where (input.StoreIds == null || input.StoreIds.Contains(m.FkStore))
                && (!(input.FkCategory > 0) || p.FkCategory == input.FkCategory)
                && (term == null || p.ProductName.ToLower().Contains(term) || (p.Sku != null && p.Sku.ToLower().Contains(term)) || (p.ProductCode != null && p.ProductCode.ToLower().Contains(term)))
            select m;

        var before = await moves.Where(m => m.CreatedAt < input.FromUtc)
            .GroupBy(m => new { m.FkProduct, Variant = m.FkProductVariant ?? 0 })
            .Select(g => new
            {
                g.Key.FkProduct,
                g.Key.Variant,
                Qty = g.Sum(m => m.MovementType == "sale" || m.MovementType == "transfer_out" || m.MovementType == "return_out" ? -m.Quantity : m.Quantity)
            }).ToListAsync(ct);

        var during = await moves.Where(m => m.CreatedAt >= input.FromUtc && m.CreatedAt < input.ToUtcExclusive)
            .GroupBy(m => new { m.FkProduct, Variant = m.FkProductVariant ?? 0, m.MovementType })
            .Select(g => new
            {
                g.Key.FkProduct,
                g.Key.Variant,
                g.Key.MovementType,
                Qty = g.Sum(m => m.Quantity)
            }).ToListAsync(ct);

        var keys = before.Select(x => (x.FkProduct, x.Variant)).Union(during.Select(x => (x.FkProduct, x.Variant))).Distinct().ToList();
        var labels = await SkuLabelsAsync(keys, ct);
        var openMap = before.ToDictionary(x => (x.FkProduct, x.Variant), x => x.Qty);
        var rowsBuilt = new List<(string Name, int Product, int Variant, decimal Open, decimal Purchase, decimal Sale, decimal RetIn, decimal RetOut, decimal Adj, decimal TrIn, decimal TrOut, decimal Close, bool Ok)>();
        foreach (var key in keys)
        {
            var open = openMap.GetValueOrDefault(key);
            decimal Of(string type) => during.Where(d => d.FkProduct == key.FkProduct && d.Variant == key.Variant && d.MovementType == type).Sum(d => d.Qty);
            var purchase = Of("purchase");
            var sale = Of("sale");
            var retIn = Of("return_in");
            var retOut = Of("return_out");
            var adj = during.Where(d => d.FkProduct == key.FkProduct && d.Variant == key.Variant && d.MovementType == "adjustment")
                .Sum(d => d.Qty);
            var trIn = Of("transfer_in");
            var trOut = Of("transfer_out");
            var openingPosted = Of("opening");
            var close = open + openingPosted + purchase + retIn + trIn + adj - sale - retOut - trOut;
            var signedDuring = during.Where(d => d.FkProduct == key.FkProduct && d.Variant == key.Variant)
                .Sum(d => SignedQty(d.MovementType, d.Qty));
            var ok = close == open + signedDuring;
            labels.TryGetValue(key, out var label);
            rowsBuilt.Add((label.Name ?? ("Item " + key.FkProduct), key.FkProduct, key.Variant, open, purchase, sale, retIn, retOut, adj + openingPosted, trIn, trOut, close, ok));
        }

        rowsBuilt = rowsBuilt.OrderBy(x => x.Name).ToList();
        var (page, size) = Paging(input);
        var pageRows = Slice(rowsBuilt, page, size).Select(x => new[]
        {
            Link(x.Name, "/Reports/ItemLedger?fkProduct=" + x.Product + (x.Variant > 0 ? "&fkProductVariant=" + x.Variant : "") + "&" + Dates(input) + StoreQ(input)),
            C(ReportFormat.Qty(x.Open)), C(ReportFormat.Qty(x.Purchase)), C(ReportFormat.Qty(x.Sale)),
            C(ReportFormat.Qty(x.RetIn)), C(ReportFormat.Qty(x.RetOut)), C(ReportFormat.Qty(x.Adj)),
            C(ReportFormat.Qty(x.TrIn)), C(ReportFormat.Qty(x.TrOut)), C(ReportFormat.Qty(x.Close), warn: !x.Ok)
        }).ToList();

        var failed = rowsBuilt.Count(x => !x.Ok);
        return new OutputReportTable
        {
            Columns = new()
            {
                Col("Item"), Col("Opening", "end"), Col("Purchase in", "end"), Col("Sales out", "end"),
                Col("Sales return in", "end"), Col("Purchase return out", "end"), Col("Adjustment", "end"),
                Col("Transfer in", "end"), Col("Transfer out", "end"), Col("Closing", "end")
            },
            Rows = pageRows,
            TotalCount = rowsBuilt.Count,
            Totals = new[]
            {
                C("Totals"),
                C(ReportFormat.Qty(rowsBuilt.Sum(x => x.Open))),
                C(ReportFormat.Qty(rowsBuilt.Sum(x => x.Purchase))),
                C(ReportFormat.Qty(rowsBuilt.Sum(x => x.Sale))),
                C(ReportFormat.Qty(rowsBuilt.Sum(x => x.RetIn))),
                C(ReportFormat.Qty(rowsBuilt.Sum(x => x.RetOut))),
                C(ReportFormat.Qty(rowsBuilt.Sum(x => x.Adj))),
                C(ReportFormat.Qty(rowsBuilt.Sum(x => x.TrIn))),
                C(ReportFormat.Qty(rowsBuilt.Sum(x => x.TrOut))),
                C(ReportFormat.Qty(rowsBuilt.Sum(x => x.Close)))
            },
            Kpis = new()
            {
                Kpi("Items", ReportFormat.Count(rowsBuilt.Count)),
                Kpi("Closing qty", ReportFormat.Qty(rowsBuilt.Sum(x => x.Close)))
            },
            Notice = failed == 0
                ? "Closing = opening + purchase + sales return + transfer in + adjustment − sales − purchase return − transfer out. In-range opening stock is included in adjustment."
                : failed + " row(s) do not satisfy opening + in − out = closing."
        };
    }

    private async Task<OutputReportTable> StockMovement(InputReportQuery input, CancellationToken ct)
    {
        var type = (input.MovementType ?? string.Empty).Trim().ToLowerInvariant();
        var q = _db.StockMovements.AsNoTracking()
            .Where(m => m.CreatedAt >= input.FromUtc && m.CreatedAt < input.ToUtcExclusive);
        if (input.StoreIds is not null)
            q = q.Where(m => input.StoreIds.Contains(m.FkStore));
        if (type.Length > 0)
            q = q.Where(m => m.MovementType == type);

        var document = string.Equals(input.GroupBy, "document", StringComparison.OrdinalIgnoreCase);
        if (!document)
        {
            var grouped = await q.GroupBy(m => m.MovementType)
                .Select(g => new
                {
                    Type = g.Key,
                    Docs = g.Select(x => x.ReferenceId ?? x.IdStockMovement).Distinct().Count(),
                    InQty = g.Sum(x => x.MovementType == "sale" || x.MovementType == "transfer_out" || x.MovementType == "return_out" || (x.MovementType == "adjustment" && x.Quantity < 0) ? 0 : (x.MovementType == "adjustment" ? x.Quantity : x.Quantity)),
                    OutQty = g.Sum(x => x.MovementType == "sale" || x.MovementType == "transfer_out" || x.MovementType == "return_out" ? x.Quantity : (x.MovementType == "adjustment" && x.Quantity < 0 ? -x.Quantity : 0))
                }).ToListAsync(ct);

            var showValue = input.CanSeeProfit;
            var costs = showValue
                ? await (
                    from m in q
                    join p in _db.Products.AsNoTracking() on m.FkProduct equals p.IdProduct
                    group new { m, p } by m.MovementType
                    into g
                    select new { Type = g.Key, Value = g.Sum(x => (x.m.Quantity < 0 ? -x.m.Quantity : x.m.Quantity) * x.p.CostPrice) }
                  ).ToDictionaryAsync(x => x.Type, x => x.Value, ct)
                : new Dictionary<string, decimal>();

            var columns = new List<ReportColumn> { Col("Movement"), Col("Documents", "end"), Col("Qty in", "end"), Col("Qty out", "end") };
            if (showValue) columns.Add(Col("Value", "end"));
            var rows = grouped.OrderBy(x => x.Type).Select(x =>
            {
                var cells = new List<ReportCell>
                {
                    C(x.Type), C(ReportFormat.Count(x.Docs)), C(ReportFormat.Qty(x.InQty)), C(ReportFormat.Qty(x.OutQty))
                };
                if (showValue) cells.Add(C(ReportFormat.Money(costs.GetValueOrDefault(x.Type))));
                return cells.ToArray();
            }).ToList();

            return new OutputReportTable
            {
                Columns = columns,
                Rows = rows,
                TotalCount = rows.Count,
                Kpis = new()
                {
                    Kpi("Qty in", ReportFormat.Qty(grouped.Sum(x => x.InQty))),
                    Kpi("Qty out", ReportFormat.Qty(grouped.Sum(x => x.OutQty)))
                },
                Notice = showValue
                    ? "Value is unsigned quantity × the current product cost. Movement rows do not store a cost."
                    : "Value is hidden without the reports.profit permission. Switch group to By document for the line list."
            };
        }

        var list = await q.OrderByDescending(m => m.CreatedAt).Select(m => new
        {
            m.CreatedAt, m.MovementType, m.Quantity, m.FkProduct, Variant = m.FkProductVariant ?? 0,
            m.ReferenceType, m.ReferenceId, m.Notes, m.CreatedBy
        }).ToListAsync(ct);
        var labels = await SkuLabelsAsync(list.Select(x => (x.FkProduct, x.Variant)).ToList(), ct);
        var docs = await DocumentLabelsAsync(list.Select(x => (x.ReferenceType, x.ReferenceId)).ToList(), ct);
        var (page, size) = Paging(input);
        var pageRows = Slice(list, page, size).Select(x =>
        {
            labels.TryGetValue((x.FkProduct, x.Variant), out var label);
            docs.TryGetValue((x.ReferenceType ?? "", x.ReferenceId ?? 0), out var doc);
            var signed = SignedQty(x.MovementType, x.Quantity);
            return new[]
            {
                C(ReportFormat.When(Local(x.CreatedAt, input.TimeZone))),
                C(x.MovementType),
                doc.Href is null ? C(doc.Label ?? x.Notes) : Link(doc.Label, doc.Href),
                Link(label.Name, "/Reports/ItemLedger?fkProduct=" + x.FkProduct + (x.Variant > 0 ? "&fkProductVariant=" + x.Variant : "") + "&" + Dates(input) + StoreQ(input)),
                C(ReportFormat.Qty(signed > 0 ? signed : 0)),
                C(ReportFormat.Qty(signed < 0 ? -signed : 0))
            };
        }).ToList();

        return new OutputReportTable
        {
            Columns = new()
            {
                Col("Date"), Col("Movement"), Col("Document"), Col("Item"), Col("In", "end"), Col("Out", "end")
            },
            Rows = pageRows,
            TotalCount = list.Count,
            Notice = "Document view lists each movement. Value is on the type view when cost is permitted."
        };
    }

    private async Task<OutputReportTable> LowStockReport(InputReportQuery input, CancellationToken ct)
    {
        var q =
            from i in _db.Inventories.AsNoTracking()
            join p in _db.Products.AsNoTracking() on i.FkProduct equals p.IdProduct
            where !p.Cancelled
                && (input.StoreIds == null || input.StoreIds.Contains(i.FkStore))
                && (!(input.FkCategory > 0) || p.FkCategory == input.FkCategory)
                && (i.ReorderLevel > 0 || p.ReorderLevel > 0)
                && i.QuantityOnHand <= (i.ReorderLevel > 0 ? i.ReorderLevel : p.ReorderLevel)
            select new
            {
                i.FkProduct,
                Variant = i.FkProductVariant ?? 0,
                p.ProductName,
                Sku = p.Sku ?? p.ProductCode,
                Qty = i.QuantityOnHand,
                Reorder = i.ReorderLevel > 0 ? i.ReorderLevel : p.ReorderLevel
            };

        if (input.FkSupplier is > 0)
        {
            var supplierId = input.FkSupplier.Value;
            var supplied = _db.PurchaseDetails.AsNoTracking()
                .Where(d => _db.Purchases.Any(pu => pu.IdPurchase == d.FkPurchase && pu.FkSupplier == supplierId && !pu.Cancelled))
                .Select(d => new { d.FkProduct, Variant = d.FkProductVariant ?? 0 });
            q = q.Where(x => supplied.Any(s => s.FkProduct == x.FkProduct && s.Variant == x.Variant));
        }

        var all = await q.OrderBy(x => x.Qty).ToListAsync(ct);
        var ids = all.Select(x => x.FkProduct).Distinct().ToList();
        var purchases = ids.Count == 0
            ? new List<(int Product, int Variant, DateOnly Date, string Supplier, decimal Cost)>()
            : (await (
                from d in _db.PurchaseDetails.AsNoTracking()
                join pu in _db.Purchases.AsNoTracking() on d.FkPurchase equals pu.IdPurchase
                join s in _db.Suppliers.AsNoTracking() on pu.FkSupplier equals s.IdSupplier
                where ids.Contains(d.FkProduct) && !pu.Cancelled
                select new { d.FkProduct, Variant = d.FkProductVariant ?? 0, pu.PurchaseDate, s.SupplierName, d.UnitCost }
            ).ToListAsync(ct)).Select(x => (Product: x.FkProduct, Variant: x.Variant, Date: x.PurchaseDate, Supplier: x.SupplierName, Cost: x.UnitCost)).ToList();

        var since = DateTime.UtcNow.AddDays(-30);
        var sold = ids.Count == 0
            ? new Dictionary<(int, int), decimal>()
            : (await (
                from d in _db.SaleDetails.AsNoTracking()
                join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
                where ids.Contains(d.FkProduct) && !s.Cancelled && s.Status == "completed" && s.SaleDate >= since
                group d by new { d.FkProduct, Variant = d.FkProductVariant ?? 0 }
                into g
                select new { g.Key.FkProduct, g.Key.Variant, Qty = g.Sum(x => x.Quantity) }
            ).ToListAsync(ct)).ToDictionary(x => (x.FkProduct, x.Variant), x => x.Qty);

        var showCost = input.CanSeeProfit;
        var (page, size) = Paging(input);
        var rows = Slice(all, page, size).Select(x =>
        {
            var last = purchases.Where(p => p.Product == x.FkProduct && p.Variant == x.Variant)
                .OrderByDescending(p => p.Date).FirstOrDefault();
            var avg = sold.GetValueOrDefault((x.FkProduct, x.Variant)) / 30m;
            var shortage = x.Reorder - x.Qty;
            var cells = new List<ReportCell>
            {
                Link(x.ProductName, "/Reports/ItemLedger?fkProduct=" + x.FkProduct + (x.Variant > 0 ? "&fkProductVariant=" + x.Variant : "") + StoreQ(input)),
                C(x.Sku), C(ReportFormat.Qty(x.Qty)), C(ReportFormat.Qty(x.Reorder)), C(ReportFormat.Qty(shortage)),
                C(last.Date == default ? "—" : ReportFormat.Day(last.Date)),
                last.Supplier is null ? C("—") : Link(last.Supplier, "/Reports/PurchaseAnalysis?groupBy=supplier")
            };
            if (showCost) cells.Add(C(last.Date == default ? "—" : ReportFormat.Money(last.Cost)));
            cells.Add(C(ReportFormat.Qty(avg)));
            cells.Add(C(ReportFormat.Qty(shortage < 0 ? 0 : shortage)));
            return cells.ToArray();
        }).ToList();

        var columns = new List<ReportColumn>
        {
            Col("Item"), Col("SKU"), Col("Qty", "end"), Col("Reorder", "end"), Col("Shortage", "end"),
            Col("Last purchase"), Col("Last supplier")
        };
        if (showCost) columns.Add(Col("Last cost", "end"));
        columns.Add(Col("Avg daily sales", "end"));
        columns.Add(Col("Suggested qty", "end"));

        return new OutputReportTable
        {
            Columns = columns,
            Rows = rows,
            TotalCount = all.Count,
            Kpis = new() { Kpi("Items", ReportFormat.Count(all.Count)) },
            Notice = "Suggested quantity brings stock back to the reorder level. Average daily sales uses completed sales from the last 30 days."
                + (showCost ? "" : " Last purchase cost is hidden without reports.profit.")
        };
    }

    private async Task<OutputReportTable> NonMoving(InputReportQuery input, CancellationToken ct)
    {
        var days = input.Days is > 0 and <= 366 ? input.Days.Value : 30;
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var term = Term(input);
        var lastSales = await _db.StockMovements.AsNoTracking()
            .Where(m => m.MovementType == "sale" && (input.StoreIds == null || input.StoreIds.Contains(m.FkStore)))
            .GroupBy(m => new { m.FkProduct, Variant = m.FkProductVariant ?? 0 })
            .Select(g => new { g.Key.FkProduct, g.Key.Variant, At = g.Max(x => x.CreatedAt) })
            .ToListAsync(ct);
        var lastMap = lastSales.ToDictionary(x => (x.FkProduct, x.Variant), x => x.At);

        var stock = await (
            from i in _db.Inventories.AsNoTracking()
            join p in _db.Products.AsNoTracking() on i.FkProduct equals p.IdProduct
            where !p.Cancelled
                && (input.StoreIds == null || input.StoreIds.Contains(i.FkStore))
                && (!(input.FkCategory > 0) || p.FkCategory == input.FkCategory)
                && (!input.OnlyInStock || i.QuantityOnHand > 0)
                && (term == null || p.ProductName.ToLower().Contains(term) || (p.Sku != null && p.Sku.ToLower().Contains(term)))
            select new
            {
                i.FkProduct,
                Variant = i.FkProductVariant ?? 0,
                Name = p.ProductName,
                Sku = p.Sku ?? p.ProductCode,
                Qty = i.QuantityOnHand,
                Cost = p.CostPrice
            }).ToListAsync(ct);

        var lastPurchases = await _db.StockMovements.AsNoTracking()
            .Where(m => m.MovementType == "purchase" && (input.StoreIds == null || input.StoreIds.Contains(m.FkStore)))
            .GroupBy(m => new { m.FkProduct, Variant = m.FkProductVariant ?? 0 })
            .Select(g => new { g.Key.FkProduct, g.Key.Variant, At = g.Max(x => x.CreatedAt) })
            .ToListAsync(ct);
        var buyMap = lastPurchases.ToDictionary(x => (x.FkProduct, x.Variant), x => x.At);

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, input.TimeZone));
        var filtered = stock.Where(x =>
        {
            if (!lastMap.TryGetValue((x.FkProduct, x.Variant), out var at))
                return true;
            return at < cutoff;
        }).ToList();

        var showCost = input.CanSeeProfit;
        var (page, size) = Paging(input);
        var rows = Slice(filtered, page, size).Select(x =>
        {
            DateTime? soldAt = lastMap.TryGetValue((x.FkProduct, x.Variant), out var s) ? s : null;
            DateTime? boughtAt = buyMap.TryGetValue((x.FkProduct, x.Variant), out var b) ? b : null;
            int? since = soldAt is DateTime sold
                ? today.DayNumber - DateOnly.FromDateTime(Local(sold, input.TimeZone)).DayNumber
                : null;
            var cells = new List<ReportCell>
            {
                Link(x.Name, "/Reports/ItemLedger?fkProduct=" + x.FkProduct + (x.Variant > 0 ? "&fkProductVariant=" + x.Variant : "") + StoreQ(input)),
                C(x.Sku), C(ReportFormat.Qty(x.Qty))
            };
            if (showCost) cells.Add(C(ReportFormat.Money(x.Qty * x.Cost)));
            cells.Add(C(soldAt is DateTime sold2 ? ReportFormat.Day(Local(sold2, input.TimeZone)) : "Never"));
            cells.Add(C(since is int d ? ReportFormat.Count(d) : "—"));
            cells.Add(C(boughtAt is DateTime bought ? ReportFormat.Day(Local(bought, input.TimeZone)) : "—"));
            return cells.ToArray();
        }).ToList();

        var columns = new List<ReportColumn> { Col("Item"), Col("SKU"), Col("Qty", "end") };
        if (showCost) columns.Add(Col("Stock value", "end"));
        columns.Add(Col("Last sold"));
        columns.Add(Col("Days since sale", "end"));
        columns.Add(Col("Last purchase"));

        return new OutputReportTable
        {
            Columns = columns,
            Rows = rows,
            TotalCount = filtered.Count,
            Kpis = new() { Kpi("Items", ReportFormat.Count(filtered.Count)), Kpi("Days", ReportFormat.Count(days)) },
            Notice = "An item is listed when its last sale movement is older than " + days + " days, or it has never sold."
                + (showCost ? " Stock value uses the current product cost." : " Stock value is hidden without reports.profit.")
        };
    }

    private async Task<OutputReportTable> StockAdjustments(InputReportQuery input, CancellationToken ct)
    {
        var term = Term(input);
        var q =
            from m in _db.StockMovements.AsNoTracking()
            join p in _db.Products.AsNoTracking() on m.FkProduct equals p.IdProduct
            join st in _db.Stores.AsNoTracking() on m.FkStore equals st.IdStore
            where m.CreatedAt >= input.FromUtc && m.CreatedAt < input.ToUtcExclusive
                && (m.MovementType == "adjustment" || m.MovementType == "transfer_in" || m.MovementType == "transfer_out")
                && (input.StoreIds == null || input.StoreIds.Contains(m.FkStore))
                && (term == null || p.ProductName.ToLower().Contains(term))
            select new
            {
                m.CreatedAt, m.MovementType, m.Quantity, m.Notes, m.FkProduct,
                Variant = m.FkProductVariant ?? 0, p.ProductName, st.StoreName, m.CreatedBy
            };

        var all = await q.OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        var userIds = all.Where(x => x.CreatedBy != null).Select(x => x.CreatedBy!.Value).Distinct().ToList();
        var users = userIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Users.AsNoTracking().Where(u => userIds.Contains(u.IdUser)).ToDictionaryAsync(u => u.IdUser, u => u.FullName, ct);
        var (page, size) = Paging(input);
        var rows = Slice(all, page, size).Select(x => new[]
        {
            C(ReportFormat.When(Local(x.CreatedAt, input.TimeZone))),
            C(x.MovementType),
            Link(x.ProductName, "/Reports/ItemLedger?fkProduct=" + x.FkProduct + (x.Variant > 0 ? "&fkProductVariant=" + x.Variant : "") + "&" + Dates(input) + StoreQ(input)),
            C(ReportFormat.Qty(SignedQty(x.MovementType, x.Quantity))),
            C(x.StoreName),
            C(x.Notes),
            C(x.CreatedBy is int uid && users.TryGetValue(uid, out var name) ? name : "—")
        }).ToList();

        return new OutputReportTable
        {
            Columns = new()
            {
                Col("Date"), Col("Type"), Col("Item"), Col("Qty", "end"), Col("Store"), Col("Notes"), Col("User")
            },
            Rows = rows,
            TotalCount = all.Count,
            Kpis = new() { Kpi("Movements", ReportFormat.Count(all.Count)) },
            Notice = "Transfer from/to is stored in the movement note, not as a separate store id. There is no adjustment reason column beyond that note."
        };
    }

    private async Task<Dictionary<(string Type, int Id), (string? Label, string? Href, string? Party)>> DocumentLabelsAsync(
        IReadOnlyCollection<(string? Type, int? Id)> refs, CancellationToken ct)
    {
        var map = new Dictionary<(string Type, int Id), (string? Label, string? Href, string? Party)>();
        var saleIds = refs.Where(r => r.Type == "sale" && r.Id is > 0).Select(r => r.Id!.Value).Distinct().ToList();
        var returnIds = refs.Where(r => r.Type == "sales_return" && r.Id is > 0).Select(r => r.Id!.Value).Distinct().ToList();
        var purchaseIds = refs.Where(r => r.Type == "purchase" && r.Id is > 0).Select(r => r.Id!.Value).Distinct().ToList();

        if (saleIds.Count > 0)
        {
            var sales = await (
                from s in _db.Sales.AsNoTracking()
                join c in _db.Customers.AsNoTracking() on s.FkCustomer equals c.IdCustomer into cj
                from c in cj.DefaultIfEmpty()
                where saleIds.Contains(s.IdSale)
                select new { s.IdSale, s.InvoiceNumber, Party = c == null ? "Walk-in" : c.FullName }
            ).ToListAsync(ct);
            foreach (var s in sales)
                map[("sale", s.IdSale)] = (s.InvoiceNumber, "/Sales/InvoiceDetails/" + s.IdSale, s.Party);
        }

        if (returnIds.Count > 0)
        {
            var returns = await _db.SalesReturns.AsNoTracking().Where(r => returnIds.Contains(r.IdSalesReturn))
                .Select(r => new { r.IdSalesReturn, r.ReturnNumber }).ToListAsync(ct);
            foreach (var r in returns)
                map[("sales_return", r.IdSalesReturn)] = (r.ReturnNumber, "/Sales/ReturnDetails/" + r.IdSalesReturn, null);
        }

        if (purchaseIds.Count > 0)
        {
            var purchases = await (
                from p in _db.Purchases.AsNoTracking()
                join s in _db.Suppliers.AsNoTracking() on p.FkSupplier equals s.IdSupplier
                where purchaseIds.Contains(p.IdPurchase)
                select new { p.IdPurchase, p.InvoiceNumber, s.SupplierName }
            ).ToListAsync(ct);
            foreach (var p in purchases)
                map[("purchase", p.IdPurchase)] = (p.InvoiceNumber ?? ("PO " + p.IdPurchase), "/Purchase/Details/" + p.IdPurchase, p.SupplierName);
        }

        return map;
    }
}
