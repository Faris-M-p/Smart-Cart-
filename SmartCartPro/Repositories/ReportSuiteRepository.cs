using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public partial class ReportSuiteRepository : IReportSuiteRepository
{
    private readonly SmartCartDbContext _db;

    public ReportSuiteRepository(SmartCartDbContext db) => _db = db;

    public async Task<List<OutputReportStoreOption>> AllowedStoresAsync(
        int userId, bool isPlatformSuperAdmin, int actorStoreId, CancellationToken ct = default)
    {
        if (isPlatformSuperAdmin)
        {
            return await _db.Stores.AsNoTracking()
                .Where(s => s.IsActive && !s.Cancelled)
                .OrderBy(s => s.StoreName)
                .Select(s => new OutputReportStoreOption { IdStore = s.IdStore, StoreName = s.StoreName })
                .ToListAsync(ct);
        }

        var ids = await _db.UserStoreAccess.AsNoTracking()
            .Where(a => a.FkUser == userId && !a.Cancelled)
            .Select(a => a.FkStore)
            .Distinct()
            .ToListAsync(ct);

        if (actorStoreId > 0 && !ids.Contains(actorStoreId))
            ids.Add(actorStoreId);

        return await _db.Stores.AsNoTracking()
            .Where(s => ids.Contains(s.IdStore) && s.IsActive && !s.Cancelled)
            .OrderBy(s => s.StoreName)
            .Select(s => new OutputReportStoreOption { IdStore = s.IdStore, StoreName = s.StoreName })
            .ToListAsync(ct);
    }

    public async Task<string> CompanyNameAsync(CancellationToken ct = default)
    {
        var name = await _db.Companies.AsNoTracking()
            .OrderBy(c => c.IdCompany)
            .Select(c => c.CompanyName)
            .FirstOrDefaultAsync(ct);
        return string.IsNullOrWhiteSpace(name) ? "SmartCart Pro" : name;
    }

    public async Task FillLookupsAsync(OutputReportScreen screen, CancellationToken ct = default)
    {
        if (screen.ShowCategory)
        {
            screen.Categories = await _db.Categories.AsNoTracking()
                .Where(c => c.IsActive && !c.Cancelled)
                .OrderBy(c => c.CategoryName)
                .Select(c => new OutputReportNamedOption { Id = c.IdCategory, Name = c.CategoryName })
                .ToListAsync(ct);
        }

        if (screen.ShowSubCategory)
        {
            var subs = _db.SubCategories.AsNoTracking().Where(s => !s.Cancelled);
            if (screen.Filter.FkCategory is > 0)
                subs = subs.Where(s => s.FkCategory == screen.Filter.FkCategory);
            screen.SubCategories = await subs.OrderBy(s => s.SubCategoryName)
                .Select(s => new OutputReportNamedOption { Id = s.IdSubcategory, Name = s.SubCategoryName })
                .ToListAsync(ct);
        }

        if (screen.ShowBrand)
        {
            screen.Brands = await _db.Brands.AsNoTracking()
                .Where(b => !b.Cancelled)
                .OrderBy(b => b.BrandName)
                .Select(b => new OutputReportNamedOption { Id = b.IdBrand, Name = b.BrandName })
                .ToListAsync(ct);
        }

        if (screen.ShowSupplier)
        {
            screen.Suppliers = await _db.Suppliers.AsNoTracking()
                .Where(s => s.IsActive && !s.Cancelled)
                .OrderBy(s => s.SupplierName)
                .Select(s => new OutputReportNamedOption { Id = s.IdSupplier, Name = s.SupplierName })
                .Take(500)
                .ToListAsync(ct);
        }

        if (screen.ShowCustomer)
        {
            screen.Customers = await _db.Customers.AsNoTracking()
                .Where(c => !c.Cancelled)
                .OrderBy(c => c.FullName)
                .Select(c => new OutputReportNamedOption { Id = c.IdCustomer, Name = c.FullName })
                .Take(500)
                .ToListAsync(ct);
        }

        if (screen.ShowCashier)
        {
            screen.Cashiers = await _db.Users.AsNoTracking()
                .Where(u => u.IsActive && !u.Cancelled && !u.IsPlatformSuperAdmin)
                .OrderBy(u => u.FullName)
                .Select(u => new OutputReportNamedOption { Id = u.IdUser, Name = u.FullName })
                .Take(300)
                .ToListAsync(ct);
        }
    }

    public Task<OutputReportTable> QueryAsync(InputReportQuery input, CancellationToken ct = default)
    {
        if (input.StoreIds is { Count: 0 })
            return Task.FromResult(new OutputReportTable { Notice = "No store is available for this sign-in." });

        return input.ReportKey switch
        {
            "SalesBillWise" => SalesBillWise(input, ct),
            "ItemSales" => ItemSales(input, ct),
            "CategorySales" => CategorySales(input, ct),
            "SalesReturns" => SalesReturns(input, ct),
            "DayClose" => DayClose(input, ct),
            "DiscountReport" => DiscountReport(input, ct),
            "CancelledBills" => CancelledBills(input, ct),
            "HourlySales" => HourlySales(input, ct),
            "CurrentStock" => CurrentStock(input, ct),
            "ItemLedger" => ItemLedger(input, ct),
            "StockLedger" => StockLedger(input, ct),
            "StockMovement" => StockMovement(input, ct),
            "LowStockReport" => LowStockReport(input, ct),
            "NonMoving" => NonMoving(input, ct),
            "StockAdjustments" => StockAdjustments(input, ct),
            "PurchaseBills" => PurchaseBills(input, ct),
            "PurchaseAnalysis" => PurchaseAnalysis(input, ct),
            "GstSummary" => GstSummary(input, ct),
            "ProfitMargin" => ProfitMargin(input, ct),
            "MasterLists" => MasterLists(input, ct),
            _ => Task.FromResult(new OutputReportTable { Notice = "This report is not available." })
        };
    }

    private sealed class LineAgg
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public decimal Qty { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Net { get; set; }
        public decimal Gross { get; set; }
        public decimal Cost { get; set; }
    }

    private static string? Term(InputReportQuery input) =>
        string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim().ToLowerInvariant();

    private static bool Desc(InputReportQuery input) =>
        !string.Equals(input.SortDir, "asc", StringComparison.OrdinalIgnoreCase);

    private static (int Page, int Size) Paging(InputReportQuery input)
    {
        if (input.ForExport)
            return (1, 20000);
        var size = input.PageSize is < 10 or > 100 ? 25 : input.PageSize;
        var page = input.Page < 1 ? 1 : input.Page;
        return (page, size);
    }

    private static List<T> Slice<T>(IReadOnlyList<T> rows, int page, int size) =>
        rows.Skip((page - 1) * size).Take(size).ToList();

    private static DateTime Local(DateTime utc, TimeZoneInfo tz)
    {
        var value = utc.Kind == DateTimeKind.Utc
            ? utc
            : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(value, tz);
    }

    private static string Dates(InputReportQuery input) =>
        "dateFrom=" + input.FromLocal.ToString("yyyy-MM-dd") + "&dateTo=" + input.ToLocal.ToString("yyyy-MM-dd");

    private static string StoreQ(InputReportQuery input) =>
        input.FkStore > 0 ? "&fkStore=" + input.FkStore : "";

    private static ReportCell C(string? text, bool muted = false, bool warn = false) =>
        new() { Text = string.IsNullOrWhiteSpace(text) ? "—" : text, Muted = muted, Warn = warn };

    private static ReportCell Link(string? text, string? href, bool muted = false) =>
        new() { Text = string.IsNullOrWhiteSpace(text) ? "—" : text, Href = href, Muted = muted };

    private static ReportColumn Col(string name, string align = "start", string? sort = null) =>
        new() { Name = name, Align = align, SortKey = sort };

    private static OutputReportKpi Kpi(string label, string value, string? hint = null) =>
        new() { Label = label, Value = value, Hint = hint };

    private static decimal SignedQty(string movementType, decimal quantity) =>
        movementType is "sale" or "transfer_out" or "return_out" ? -quantity : quantity;

    private async Task<List<LineAgg>> LoadSoldAsync(InputReportQuery input, CancellationToken ct)
    {
        var term = Term(input);
        return await (
            from d in _db.SaleDetails.AsNoTracking()
            join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
            join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
            join v in _db.ProductVariants.AsNoTracking() on d.FkProductVariant equals (int?)v.IdProductVariant into vj
            from v in vj.DefaultIfEmpty()
            where !s.Cancelled && s.Status == "completed"
                && s.SaleDate >= input.FromUtc && s.SaleDate < input.ToUtcExclusive
                && (input.StoreIds == null || input.StoreIds.Contains(s.FkStore))
                && (!(input.FkCategory > 0) || p.FkCategory == input.FkCategory)
                && (!(input.FkSubCategory > 0) || p.FkSubcategory == input.FkSubCategory)
                && (!(input.FkBrand > 0) || p.FkBrand == input.FkBrand)
                && (term == null
                    || p.ProductName.ToLower().Contains(term)
                    || (p.ProductCode != null && p.ProductCode.ToLower().Contains(term))
                    || (p.Sku != null && p.Sku.ToLower().Contains(term))
                    || (p.Barcode != null && p.Barcode.ToLower().Contains(term))
                    || (v != null && v.VariantName.ToLower().Contains(term))
                    || (v != null && v.SkuCode != null && v.SkuCode.ToLower().Contains(term)))
            group d by new
            {
                d.FkProduct,
                Variant = d.FkProductVariant ?? 0,
                Category = p.FkCategory ?? 0,
                Sub = p.FkSubcategory ?? 0,
                p.CostPrice
            }
            into g
            select new LineAgg
            {
                ProductId = g.Key.FkProduct,
                VariantId = g.Key.Variant,
                CategoryId = g.Key.Category,
                SubCategoryId = g.Key.Sub,
                Cost = g.Key.CostPrice,
                Qty = g.Sum(x => x.Quantity),
                Discount = g.Sum(x => x.DiscountAmount),
                Tax = g.Sum(x => x.TaxAmount),
                Net = g.Sum(x => x.LineTotal),
                Gross = g.Sum(x => x.LineTotal - x.TaxAmount + x.DiscountAmount)
            }).ToListAsync(ct);
    }

    private async Task<Dictionary<(int Product, int Variant), (decimal Qty, decimal Value)>> LoadReturnsAsync(
        InputReportQuery input, CancellationToken ct)
    {
        var rows = await (
            from d in _db.SalesReturnDetails.AsNoTracking()
            join r in _db.SalesReturns.AsNoTracking() on d.FkSalesReturn equals r.IdSalesReturn
            where !r.Cancelled
                && r.ReturnDate >= input.FromUtc && r.ReturnDate < input.ToUtcExclusive
                && (input.StoreIds == null || input.StoreIds.Contains(r.FkStore))
            group d by new { d.FkProduct, Variant = d.FkProductVariant ?? 0 }
            into g
            select new
            {
                g.Key.FkProduct,
                g.Key.Variant,
                Qty = g.Sum(x => x.Quantity),
                Value = g.Sum(x => x.LineTotal)
            }).ToListAsync(ct);

        return rows.ToDictionary(x => (Product: x.FkProduct, Variant: x.Variant), x => (Qty: x.Qty, Value: x.Value));
    }
}
