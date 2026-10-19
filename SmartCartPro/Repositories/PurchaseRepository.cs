using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Repositories;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly SmartCartDbContext _db;
    private readonly IStockRepository _stock;
    private readonly IDocumentNumberService _docNum;

    public PurchaseRepository(SmartCartDbContext db, IStockRepository stock, IDocumentNumberService docNum)
    {
        _db = db;
        _stock = stock;
        _docNum = docNum;
    }

    public async Task<OutputPurchaseSelect?> GetPurchaseAsync(InputPurchaseSelect input, CancellationToken ct = default)
    {
        var po = await _db.Purchases
            .Include(x => x.Supplier)
            .Include(x => x.Store)
            .Include(x => x.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(x => x.IdPurchase == input.IdPurchase && !x.Cancelled, ct);

        if (po is null) return null;

        return new OutputPurchaseSelect
        {
            IdPurchase = po.IdPurchase,
            FkStore = po.FkStore,
            StoreName = po.Store?.StoreName ?? string.Empty,
            FkSupplier = po.FkSupplier,
            SupplierName = po.Supplier?.SupplierName ?? string.Empty,
            PurchaseDate = po.PurchaseDate,
            InvoiceNumber = po.InvoiceNumber,
            SubTotal = po.SubTotal,
            TaxAmount = po.TaxAmount,
            DiscountAmount = po.DiscountAmount,
            TotalAmount = po.TotalAmount,
            Status = po.Status,
            Notes = po.Notes,
            CreatedAt = po.CreatedAt,
            Lines = po.Details.Select(d => new OutputPurchaseLine
            {
                IdPurchaseDetail = d.IdPurchaseDetail,
                FkProduct = d.FkProduct,
                ProductName = d.Product?.ProductName ?? string.Empty,
                ProductCode = d.Product?.ProductCode,
                FkProductVariant = d.FkProductVariant,
                Quantity = d.Quantity,
                UnitCost = d.UnitCost,
                TaxAmount = d.TaxAmount,
                LineTotal = d.LineTotal
            }).ToList()
        };
    }

    public async Task<OutputPurchaseList> ListPurchasesAsync(InputPurchaseList input, CancellationToken ct = default)
    {
        var q = _db.Purchases
            .Include(x => x.Supplier)
            .Include(x => x.Store)
            .Where(x => !x.Cancelled);

        if (input.FkStore.HasValue) q = q.Where(x => x.FkStore == input.FkStore.Value);
        if (input.FkSupplier.HasValue) q = q.Where(x => x.FkSupplier == input.FkSupplier.Value);
        if (!string.IsNullOrWhiteSpace(input.Status)) q = q.Where(x => x.Status == input.Status);
        if (!string.IsNullOrWhiteSpace(input.Search))
            q = q.Where(x => x.InvoiceNumber != null && x.InvoiceNumber.Contains(input.Search));
        if (input.From.HasValue) q = q.Where(x => x.CreatedAt >= input.From.Value);
        if (input.To.HasValue) q = q.Where(x => x.CreatedAt <= input.To.Value);

        q = input.SortDesc ? q.OrderByDescending(x => x.CreatedAt) : q.OrderBy(x => x.CreatedAt);

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(x => new OutputPurchaseListItem
            {
                IdPurchase = x.IdPurchase,
                FkStore = x.FkStore,
                StoreName = x.Store != null ? x.Store.StoreName : string.Empty,
                SupplierName = x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                PurchaseDate = x.PurchaseDate,
                InvoiceNumber = x.InvoiceNumber,
                TotalAmount = x.TotalAmount,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return new OutputPurchaseList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
    }

    public async Task<OutputPurchaseCreate> CreatePurchaseAsync(InputPurchaseCreate input, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var poNumber = await _docNum.NextAsync(input.FkStore, "PO", ct);

            var po = new Purchase
            {
                FkStore = input.FkStore,
                FkSupplier = input.FkSupplier,
                PurchaseDate = input.PurchaseDate,
                InvoiceNumber = input.InvoiceNumber,
                Notes = input.Notes,
                Status = "posted",
                CreatedBy = input.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            // Recalculate totals server-side
            decimal subTotal = 0, taxTotal = 0;
            var details = new List<PurchaseDetail>();
            foreach (var line in input.Lines)
            {
                // Load tax rate if product has one
                var taxRate = await _db.Products
                    .Where(p => p.IdProduct == line.FkProduct && p.FkTax != null)
                    .Join(_db.Taxes, p => p.FkTax, t => t.IdTax, (p, t) => t.TaxRate)
                    .FirstOrDefaultAsync(ct);

                var lineSubtotal = line.Quantity * line.UnitCost;
                var lineTax = taxRate > 0 ? Math.Round(lineSubtotal * taxRate / 100, 2) : 0;
                var lineTotal = lineSubtotal + lineTax;

                subTotal += lineSubtotal;
                taxTotal += lineTax;

                details.Add(new PurchaseDetail
                {
                    FkStore = input.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    Quantity = line.Quantity,
                    UnitCost = line.UnitCost,
                    TaxAmount = lineTax,
                    LineTotal = lineTotal,
                    CreatedAt = DateTime.UtcNow
                });
            }

            po.SubTotal = subTotal;
            po.TaxAmount = taxTotal;
            po.TotalAmount = subTotal + taxTotal;

            _db.Purchases.Add(po);
            await _db.SaveChangesAsync(ct);

            foreach (var d in details) { d.FkPurchase = po.IdPurchase; }
            _db.PurchaseDetails.AddRange(details);
            await _db.SaveChangesAsync(ct);

            await tx.CommitAsync(ct);
            return new OutputPurchaseCreate { IdPurchase = po.IdPurchase };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<OutputPurchaseUpdate> UpdatePurchaseAsync(InputPurchaseUpdate input, CancellationToken ct = default)
    {
        var po = await _db.Purchases.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.IdPurchase == input.IdPurchase && !x.Cancelled, ct);
        if (po is null) return new OutputPurchaseUpdate { ResponseCode = 1, ResponseMsg = "Purchase not found.", StatusCode = 404 };
        if (po.Status != "posted") return new OutputPurchaseUpdate { ResponseCode = 2, ResponseMsg = "Only draft/posted POs can be edited.", StatusCode = 422 };

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            // Remove old details
            _db.PurchaseDetails.RemoveRange(po.Details);
            await _db.SaveChangesAsync(ct);

            po.FkSupplier = input.FkSupplier;
            po.PurchaseDate = input.PurchaseDate;
            po.InvoiceNumber = input.InvoiceNumber;
            po.Notes = input.Notes;
            po.UpdatedAt = DateTime.UtcNow;

            decimal subTotal = 0, taxTotal = 0;
            var details = new List<PurchaseDetail>();
            foreach (var line in input.Lines)
            {
                var taxRate = await _db.Products
                    .Where(p => p.IdProduct == line.FkProduct && p.FkTax != null)
                    .Join(_db.Taxes, p => p.FkTax, t => t.IdTax, (p, t) => t.TaxRate)
                    .FirstOrDefaultAsync(ct);

                var lineSubtotal = line.Quantity * line.UnitCost;
                var lineTax = taxRate > 0 ? Math.Round(lineSubtotal * taxRate / 100, 2) : 0;

                subTotal += lineSubtotal;
                taxTotal += lineTax;

                details.Add(new PurchaseDetail
                {
                    FkPurchase = po.IdPurchase,
                    FkStore = po.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    Quantity = line.Quantity,
                    UnitCost = line.UnitCost,
                    TaxAmount = lineTax,
                    LineTotal = lineSubtotal + lineTax,
                    CreatedAt = DateTime.UtcNow
                });
            }

            po.SubTotal = subTotal;
            po.TaxAmount = taxTotal;
            po.TotalAmount = subTotal + taxTotal;

            _db.PurchaseDetails.AddRange(details);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new OutputPurchaseUpdate { IdPurchase = po.IdPurchase };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<OutputGoodsReceipt> ReceiveGoodsAsync(InputGoodsReceipt input, CancellationToken ct = default)
    {
        var po = await _db.Purchases.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.IdPurchase == input.IdPurchase && !x.Cancelled, ct);

        if (po is null) return new OutputGoodsReceipt { ResponseCode = 1, ResponseMsg = "Purchase not found.", StatusCode = 404 };
        if (po.Status == "cancelled") return new OutputGoodsReceipt { ResponseCode = 2, ResponseMsg = "Cannot receive a cancelled PO.", StatusCode = 422 };
        if (po.Status == "received") return new OutputGoodsReceipt { ResponseCode = 3, ResponseMsg = "PO already fully received.", StatusCode = 422 };

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var grnNumber = await _docNum.NextAsync(po.FkStore, "GRN", ct);

            foreach (var line in input.Lines.Where(l => l.ReceiveQty > 0))
            {
                var result = await _stock.ApplyMovementAsync(new InputStockMovement
                {
                    FkStore = po.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    MovementType = "purchase",
                    Quantity = line.ReceiveQty,
                    ReferenceType = "purchase",
                    ReferenceId = po.IdPurchase,
                    Notes = $"GRN {grnNumber}",
                    CreatedBy = input.CreatedBy
                }, ct);

                if (!result.Ok)
                {
                    await tx.RollbackAsync(ct);
                    return new OutputGoodsReceipt { ResponseCode = result.ResponseCode, ResponseMsg = result.ResponseMsg, StatusCode = result.StatusCode };
                }
            }

            // Update PO status
            bool allReceived = po.Details.All(d =>
                input.Lines.Any(l => l.IdPurchaseDetail == d.IdPurchaseDetail));

            po.Status = allReceived ? "received" : "partially_received";
            po.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new OutputGoodsReceipt { GrnNumber = grnNumber, NewPurchaseStatus = po.Status };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<OutputPurchaseCancel> CancelPurchaseAsync(InputPurchaseCancel input, CancellationToken ct = default)
    {
        var po = await _db.Purchases.FirstOrDefaultAsync(x => x.IdPurchase == input.IdPurchase && !x.Cancelled, ct);
        if (po is null) return new OutputPurchaseCancel { ResponseCode = 1, ResponseMsg = "Purchase not found.", StatusCode = 404 };
        if (po.Status == "received") return new OutputPurchaseCancel { ResponseCode = 2, ResponseMsg = "Cannot cancel a fully-received PO.", StatusCode = 422 };

        po.Cancelled = true;
        po.CancelledOn = DateTime.UtcNow;
        po.CancelledReason = input.Reason;
        po.Status = "cancelled";
        po.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputPurchaseCancel();
    }
}
