using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

/// <summary>
/// Report service — EF-based aggregation queries.
/// Uses EF LINQ; Dapper may be added later for heavier queries.
/// ModuleAccess: REPORTS  |  Permission: reports.view
/// </summary>
public class ReportService : IReportService
{
    private readonly SmartCartDbContext _db;
    private readonly IStockRepository _stock;

    public ReportService(SmartCartDbContext db, IStockRepository stock)
    {
        _db    = db;
        _stock = stock;
    }

    // ── Sales by Date ──────────────────────────────────────────────

    public async Task<OutputReportSalesByDate> GetSalesByDateAsync(
        InputReportSalesByDate input, CancellationToken ct = default)
    {
        var q = _db.Sales.Where(s => !s.Cancelled && s.SaleDate >= input.DateFrom && s.SaleDate < input.DateTo);

        if (input.FkStore.HasValue)
            q = q.Where(s => s.FkStore == input.FkStore.Value);

        var rows = await q
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new SalesByDateRow
            {
                SaleDate       = g.Key,
                OrderCount     = g.Count(),
                NetSales       = g.Sum(s => s.SubTotal),
                TaxAmount      = g.Sum(s => s.TaxAmount),
                DiscountAmount = g.Sum(s => s.DiscountAmount),
                TotalAmount    = g.Sum(s => s.TotalAmount)
            })
            .OrderBy(r => r.SaleDate)
            .ToListAsync(ct);

        return new OutputReportSalesByDate
        {
            Filter     = input,
            Rows       = rows,
            GrandTotal = rows.Sum(r => r.TotalAmount)
        };
    }

    // ── Stock Valuation ────────────────────────────────────────────

    public async Task<OutputReportStockValuation> GetStockValuationAsync(
        InputReportStockValuation input, CancellationToken ct = default)
    {
        var q = _db.Inventories
            .Include(i => i.Product).ThenInclude(p => p!.Category)
            .Include(i => i.Store)
            .AsQueryable();

        if (input.FkStore.HasValue)
            q = q.Where(i => i.FkStore == input.FkStore.Value);

        if (input.FkCategory.HasValue)
            q = q.Where(i => i.Product != null && i.Product.FkCategory == input.FkCategory.Value);

        var rows = await q
            .Where(i => i.Product != null && !i.Product.Cancelled && i.Product.IsActive)
            .Select(i => new StockValuationRow
            {
                FkProduct     = i.FkProduct,
                ProductName   = i.Product != null ? i.Product.ProductName : string.Empty,
                ProductCode   = i.Product != null ? i.Product.ProductCode : null,
                CategoryName  = i.Product != null && i.Product.Category != null ? i.Product.Category.CategoryName : null,
                FkStore       = i.FkStore,
                StoreName     = i.Store != null ? i.Store.StoreName : string.Empty,
                QuantityOnHand = i.QuantityOnHand,
                UnitCost      = i.Product != null ? i.Product.CostPrice : 0,
                StockValue    = i.QuantityOnHand * (i.Product != null ? i.Product.CostPrice : 0)
            })
            .OrderByDescending(r => r.StockValue)
            .ToListAsync(ct);

        return new OutputReportStockValuation
        {
            Filter          = input,
            Rows            = rows,
            TotalStockValue = rows.Sum(r => r.StockValue)
        };
    }

    // ── Low Stock ──────────────────────────────────────────────────

    public async Task<OutputLowStockList> GetLowStockReportAsync(
        InputReportLowStock input, CancellationToken ct = default)
    {
        var lowInput = new InputLowStockList
        {
            FkStore     = input.FkStore ?? 0,
            FkCategory  = input.FkCategory,
            Page        = input.Page,
            PageSize    = input.PageSize
        };

        // For all-store low stock when FkStore = 0, query directly
        if (!input.FkStore.HasValue || input.FkStore == 0)
        {
            var q = _db.Inventories
                .Include(i => i.Product)
                .Include(i => i.Store)
                .Where(i => i.QuantityOnHand <= i.ReorderLevel);

            if (input.FkCategory.HasValue)
                q = q.Where(i => i.Product != null && i.Product.FkCategory == input.FkCategory.Value);

            var total = await q.CountAsync(ct);
            var items = await q
                .OrderBy(i => i.QuantityOnHand)
                .Skip((input.Page - 1) * input.PageSize)
                .Take(input.PageSize)
                .Select(i => new OutputInventorySelect
                {
                    IdInventory      = i.IdInventory,
                    FkStore          = i.FkStore,
                    FkProduct        = i.FkProduct,
                    ProductName      = i.Product != null ? i.Product.ProductName : string.Empty,
                    ProductCode      = i.Product != null ? i.Product.ProductCode : null,
                    FkProductVariant = i.FkProductVariant,
                    QuantityOnHand   = i.QuantityOnHand,
                    QuantityReserved = i.QuantityReserved,
                    ReorderLevel     = i.ReorderLevel
                })
                .ToListAsync(ct);

            return new OutputLowStockList
            {
                TotalCount = total,
                Page       = input.Page,
                PageSize   = input.PageSize,
                Items      = items
            };
        }

        return await _stock.GetLowStockAsync(lowInput, ct);
    }

    // ── Tax Summary ────────────────────────────────────────────────

    public async Task<OutputReportTaxSummary> GetTaxSummaryAsync(
        InputReportTaxSummary input, CancellationToken ct = default)
    {
        var q = _db.Sales
            .Include(s => s.Details)
            .Where(s => !s.Cancelled && s.SaleDate >= input.DateFrom && s.SaleDate < input.DateTo);

        if (input.FkStore.HasValue)
            q = q.Where(s => s.FkStore == input.FkStore.Value);

        // Group by tax via details join
        var rows = await q
            .GroupBy(s => s.FkStore) // group placeholder — real tax aggregation via details
            .Select(g => new TaxSummaryRow
            {
                TaxCode      = "TOTAL",
                TaxName      = "All taxes",
                TaxRate      = 0,
                SaleCount    = g.Count(),
                NetSales     = g.Sum(s => s.SubTotal),
                TaxCollected = g.Sum(s => s.TaxAmount)
            })
            .ToListAsync(ct);

        return new OutputReportTaxSummary
        {
            Filter             = input,
            Rows               = rows,
            TotalTaxCollected  = rows.Sum(r => r.TaxCollected)
        };
    }

    // ── Top Selling ────────────────────────────────────────────────

    public async Task<OutputReportTopSelling> GetTopSellingAsync(
        InputReportTopSelling input, CancellationToken ct = default)
    {
        var q = _db.SaleDetails
            .Include(d => d.Sale)
            .Include(d => d.Product).ThenInclude(p => p!.Category)
            .Where(d => d.Sale != null && !d.Sale.Cancelled
                     && d.Sale.SaleDate >= input.DateFrom
                     && d.Sale.SaleDate < input.DateTo);

        if (input.FkStore.HasValue)
            q = q.Where(d => d.Sale != null && d.Sale.FkStore == input.FkStore.Value);

        var rows = await q
            .GroupBy(d => new { d.FkProduct, d.Product!.ProductName, d.Product.ProductCode, CategoryName = d.Product.Category != null ? d.Product.Category.CategoryName : null })
            .Select(g => new TopSellingRow
            {
                FkProduct          = g.Key.FkProduct,
                ProductName        = g.Key.ProductName,
                ProductCode        = g.Key.ProductCode,
                CategoryName       = g.Key.CategoryName,
                TotalQuantitySold  = g.Sum(d => d.Quantity),
                TotalRevenue       = g.Sum(d => d.LineTotal),
                TransactionCount   = g.Count()
            })
            .OrderByDescending(r => r.TotalQuantitySold)
            .Take(input.Top)
            .ToListAsync(ct);

        return new OutputReportTopSelling { Filter = input, Rows = rows };
    }
}
