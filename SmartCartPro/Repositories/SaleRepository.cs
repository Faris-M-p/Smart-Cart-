using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly SmartCartDbContext _db;
    private readonly IStockRepository _stock;
    private readonly IDocumentNumberService _docNum;

    public SaleRepository(SmartCartDbContext db, IStockRepository stock, IDocumentNumberService docNum)
    {
        _db = db;
        _stock = stock;
        _docNum = docNum;
    }

    // ═══════════════════════════════════════════════════════════════
    // GET / LIST
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputSaleSelect?> GetSaleAsync(InputSaleSelect input, CancellationToken ct = default)
    {
        var sale = await _db.Sales
            .Include(x => x.Store)
            .Include(x => x.Customer)
            .Include(x => x.Details).ThenInclude(d => d.Product)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.IdSale == input.IdSale, ct);

        if (sale is null) return null;

        return MapSaleSelect(sale);
    }

    public async Task<OutputSaleList> ListSalesAsync(InputSaleList input, CancellationToken ct = default)
    {
        var q = _db.Sales
            .Include(x => x.Customer)
            .Where(x => !x.Cancelled);

        if (input.FkStore.HasValue) q = q.Where(x => x.FkStore == input.FkStore.Value);
        if (input.FkCustomer.HasValue) q = q.Where(x => x.FkCustomer == input.FkCustomer.Value);
        if (!string.IsNullOrWhiteSpace(input.Status)) q = q.Where(x => x.Status == input.Status);
        if (!string.IsNullOrWhiteSpace(input.Search))
            q = q.Where(x => x.InvoiceNumber.Contains(input.Search));
        if (input.From.HasValue) q = q.Where(x => x.SaleDate >= input.From.Value);
        if (input.To.HasValue) q = q.Where(x => x.SaleDate <= input.To.Value);

        q = input.SortDesc ? q.OrderByDescending(x => x.SaleDate) : q.OrderBy(x => x.SaleDate);

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(x => new OutputSaleListItem
            {
                IdSale = x.IdSale,
                InvoiceNumber = x.InvoiceNumber,
                SaleDate = x.SaleDate,
                CustomerName = x.Customer != null ? x.Customer.FullName : null,
                TotalAmount = x.TotalAmount,
                PaidAmount = x.PaidAmount,
                Status = x.Status
            })
            .ToListAsync(ct);

        return new OutputSaleList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE SALE
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputSaleCreate> CreateSaleAsync(InputSaleCreate input, CancellationToken ct = default)
    {
        // Idempotency: check if client ref already exists
        if (!string.IsNullOrWhiteSpace(input.ClientRef))
        {
            var existing = await _db.Sales.FirstOrDefaultAsync(
                x => x.FkStore == input.FkStore && x.Notes == $"__ref:{input.ClientRef}", ct);
            if (existing is not null)
                return new OutputSaleCreate
                {
                    IdSale = existing.IdSale,
                    InvoiceNumber = existing.InvoiceNumber,
                    TotalAmount = existing.TotalAmount,
                    PaidAmount = existing.PaidAmount,
                    ChangeGiven = existing.ChangeGiven
                };
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var invoiceNumber = await _docNum.NextAsync(input.FkStore, "SALE", ct);

            // Recalculate all totals server-side
            decimal subTotal = 0, taxTotal = 0, discTotal = 0;
            var details = new List<SaleDetail>();

            foreach (var line in input.Lines)
            {
                // Fetch product with tax info
                var product = await _db.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.IdProduct == line.FkProduct && !p.Cancelled, ct);

                if (product is null)
                {
                    await tx.RollbackAsync(ct);
                    return new OutputSaleCreate { ResponseCode = 1, ResponseMsg = $"Product {line.FkProduct} not found.", StatusCode = 422 };
                }

                decimal unitPrice = line.UnitPrice > 0 ? line.UnitPrice!.Value : product.SellingPrice;
                decimal lineSubtotal = line.Quantity * unitPrice;
                decimal lineDiscount = line.DiscountAmount;

                // Fetch tax rate
                decimal taxRate = 0;
                if (product.FkTax.HasValue)
                    taxRate = await _db.Taxes.Where(t => t.IdTax == product.FkTax).Select(t => t.TaxRate).FirstOrDefaultAsync(ct);

                decimal lineTax = taxRate > 0 ? Math.Round((lineSubtotal - lineDiscount) * taxRate / 100, 2) : 0;
                decimal lineTotal = lineSubtotal - lineDiscount + lineTax;

                subTotal += lineSubtotal;
                taxTotal += lineTax;
                discTotal += lineDiscount;

                // Deduct stock
                var stockResult = await _stock.ApplyMovementAsync(new InputStockMovement
                {
                    FkStore = input.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    MovementType = "sale",
                    Quantity = line.Quantity,
                    ReferenceType = "sale",
                    Notes = $"Sale {invoiceNumber}",
                    CreatedBy = input.CreatedBy
                }, ct);

                if (!stockResult.Ok)
                {
                    await tx.RollbackAsync(ct);
                    return new OutputSaleCreate { ResponseCode = stockResult.ResponseCode, ResponseMsg = stockResult.ResponseMsg, StatusCode = stockResult.StatusCode };
                }

                details.Add(new SaleDetail
                {
                    FkStore = input.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    Quantity = line.Quantity,
                    UnitPrice = unitPrice,
                    TaxAmount = lineTax,
                    DiscountAmount = lineDiscount,
                    LineTotal = lineTotal,
                    CreatedAt = DateTime.UtcNow
                });
            }

            decimal totalAmount = subTotal - discTotal + taxTotal;
            decimal paidAmount = input.Payments.Sum(p => p.Amount);
            decimal changeGiven = Math.Max(0, paidAmount - totalAmount);

            var sale = new Sale
            {
                FkStore = input.FkStore,
                FkCustomer = input.FkCustomer,
                FkCashSession = input.FkCashSession,
                InvoiceNumber = invoiceNumber,
                SaleDate = DateTime.UtcNow,
                SubTotal = subTotal,
                TaxAmount = taxTotal,
                DiscountAmount = discTotal,
                TotalAmount = totalAmount,
                PaidAmount = paidAmount,
                ChangeGiven = changeGiven,
                Status = "completed",
                Notes = !string.IsNullOrWhiteSpace(input.ClientRef) ? $"__ref:{input.ClientRef}" : input.Notes,
                CreatedBy = input.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _db.Sales.Add(sale);
            await _db.SaveChangesAsync(ct);

            foreach (var d in details) { d.FkSale = sale.IdSale; }
            _db.SaleDetails.AddRange(details);

            // Record payments
            foreach (var pay in input.Payments)
            {
                _db.Payments.Add(new Payment
                {
                    FkStore = input.FkStore,
                    FkSale = sale.IdSale,
                    FkCashSession = input.FkCashSession,
                    PaymentMethod = pay.PaymentMethod,
                    Amount = pay.Amount,
                    ReferenceNo = pay.ReferenceNo,
                    PaidAt = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new OutputSaleCreate
            {
                IdSale = sale.IdSale,
                InvoiceNumber = sale.InvoiceNumber,
                TotalAmount = sale.TotalAmount,
                PaidAmount = sale.PaidAmount,
                ChangeGiven = sale.ChangeGiven
            };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // CANCEL
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputSaleCancel> CancelSaleAsync(InputSaleCancel input, CancellationToken ct = default)
    {
        var sale = await _db.Sales.Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.IdSale == input.IdSale && x.FkStore == input.FkStore, ct);

        if (sale is null) return new OutputSaleCancel { ResponseCode = 1, ResponseMsg = "Sale not found.", StatusCode = 404 };
        if (sale.Cancelled) return new OutputSaleCancel { ResponseCode = 2, ResponseMsg = "Already cancelled.", StatusCode = 422 };

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            // Reverse stock for each line
            foreach (var line in sale.Details)
            {
                await _stock.ApplyMovementAsync(new InputStockMovement
                {
                    FkStore = sale.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    MovementType = "return_in",
                    Quantity = line.Quantity,
                    ReferenceType = "sale_cancel",
                    ReferenceId = sale.IdSale,
                    Notes = $"Cancellation of {sale.InvoiceNumber}",
                    CreatedBy = input.CancelledBy
                }, ct);
            }

            sale.Cancelled = true;
            sale.CancelledOn = DateTime.UtcNow;
            sale.CancelledReason = input.Reason;
            sale.Status = "cancelled";
            sale.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new OutputSaleCancel();
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // RETURN
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputSaleReturn> ReturnSaleAsync(InputSaleReturn input, CancellationToken ct = default)
    {
        var sale = await _db.Sales.FirstOrDefaultAsync(x => x.IdSale == input.IdSale && x.FkStore == input.FkStore, ct);
        if (sale is null) return new OutputSaleReturn { ResponseCode = 1, ResponseMsg = "Sale not found.", StatusCode = 404 };

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var returnNumber = await _docNum.NextAsync(input.FkStore, "RETURN", ct);
            decimal totalReturn = 0;

            var salesReturn = new SalesReturn
            {
                FkStore = input.FkStore,
                FkSale = sale.IdSale,
                ReturnNumber = returnNumber,
                ReturnDate = DateTime.UtcNow,
                Reason = input.Reason,
                Status = "completed",
                CreatedBy = input.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            _db.SalesReturns.Add(salesReturn);
            await _db.SaveChangesAsync(ct);

            foreach (var line in input.Lines)
            {
                var lineTotal = line.ReturnQty * line.UnitPrice;
                totalReturn += lineTotal;

                _db.SalesReturnDetails.Add(new SalesReturnDetail
                {
                    FkSalesReturn = salesReturn.IdSalesReturn,
                    FkStore = input.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    Quantity = line.ReturnQty,
                    UnitPrice = line.UnitPrice,
                    LineTotal = lineTotal,
                    CreatedAt = DateTime.UtcNow
                });

                // Return stock
                await _stock.ApplyMovementAsync(new InputStockMovement
                {
                    FkStore = input.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    MovementType = "return_in",
                    Quantity = line.ReturnQty,
                    ReferenceType = "sales_return",
                    ReferenceId = salesReturn.IdSalesReturn,
                    Notes = $"Return {returnNumber}",
                    CreatedBy = input.CreatedBy
                }, ct);
            }

            salesReturn.TotalAmount = totalReturn;
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new OutputSaleReturn { IdSalesReturn = salesReturn.IdSalesReturn, ReturnNumber = returnNumber, TotalAmount = totalReturn };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // CASH SESSION
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputCashSessionOpen> OpenSessionAsync(InputCashSessionOpen input, CancellationToken ct = default)
    {
        var existing = await _db.CashSessions.FirstOrDefaultAsync(
            x => x.FkStore == input.FkStore && x.Status == "open", ct);
        if (existing is not null)
            return new OutputCashSessionOpen { ResponseCode = 1, ResponseMsg = "A session is already open for this store.", StatusCode = 422 };

        var session = new CashSession
        {
            FkStore = input.FkStore,
            FkUser = input.FkUser,
            OpenedAt = DateTime.UtcNow,
            OpeningFloat = input.OpeningFloat,
            Status = "open",
            Notes = input.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _db.CashSessions.Add(session);
        await _db.SaveChangesAsync(ct);
        return new OutputCashSessionOpen { IdCashSession = session.IdCashSession };
    }

    public async Task<OutputCashSessionClose> CloseSessionAsync(InputCashSessionClose input, CancellationToken ct = default)
    {
        var session = await _db.CashSessions.FirstOrDefaultAsync(
            x => x.IdCashSession == input.IdCashSession && x.FkStore == input.FkStore && x.Status == "open", ct);
        if (session is null)
            return new OutputCashSessionClose { ResponseCode = 1, ResponseMsg = "Open session not found.", StatusCode = 404 };

        // Calculate expected cash: opening float + all cash payments
        decimal cashIn = await _db.Payments
            .Where(x => x.FkCashSession == session.IdCashSession && !x.Cancelled && x.PaymentMethod == "cash")
            .SumAsync(x => (decimal?)x.Amount, ct) ?? 0;

        decimal expectedCash = session.OpeningFloat + cashIn;
        decimal variance = input.ClosingCash - expectedCash;

        session.Status = "closed";
        session.ClosedAt = DateTime.UtcNow;
        session.ClosingCash = input.ClosingCash;
        session.ExpectedCash = expectedCash;
        session.Variance = variance;
        session.Notes = input.Notes;
        session.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputCashSessionClose { ExpectedCash = expectedCash, ClosingCash = input.ClosingCash, Variance = variance };
    }

    public async Task<OutputCashSessionSelect?> GetOpenSessionAsync(InputCashSessionSelect input, CancellationToken ct = default)
    {
        var q = _db.CashSessions.Include(x => x.User).Where(x => x.FkStore == input.FkStore);
        if (input.IdCashSession.HasValue)
            q = q.Where(x => x.IdCashSession == input.IdCashSession.Value);
        else
            q = q.Where(x => x.Status == "open");

        return await q.Select(x => new OutputCashSessionSelect
        {
            IdCashSession = x.IdCashSession,
            FkStore = x.FkStore,
            FkUser = x.FkUser,
            UserName = x.User != null ? x.User.UserName : string.Empty,
            OpenedAt = x.OpenedAt,
            ClosedAt = x.ClosedAt,
            OpeningFloat = x.OpeningFloat,
            ClosingCash = x.ClosingCash,
            ExpectedCash = x.ExpectedCash,
            Variance = x.Variance,
            Status = x.Status,
            Notes = x.Notes
        }).FirstOrDefaultAsync(ct);
    }

    public async Task<OutputCashSummary> GetCashSummaryAsync(InputCashSummary input, CancellationToken ct = default)
    {
        var session = await _db.CashSessions
            .FirstOrDefaultAsync(x => x.IdCashSession == input.IdCashSession && x.FkStore == input.FkStore, ct);
        if (session is null) return new OutputCashSummary();

        var sales = await _db.Sales.Where(x => x.FkCashSession == input.IdCashSession && !x.Cancelled).ToListAsync(ct);
        var returns = await _db.SalesReturns.Where(x =>
            x.FkStore == input.FkStore && !x.Cancelled &&
            x.ReturnDate >= session.OpenedAt &&
            (session.ClosedAt == null || x.ReturnDate <= session.ClosedAt)).SumAsync(x => (decimal?)x.TotalAmount, ct) ?? 0;

        var byMethod = await _db.Payments
            .Where(x => x.FkCashSession == input.IdCashSession && !x.Cancelled)
            .GroupBy(x => x.PaymentMethod)
            .Select(g => new OutputCashSummaryPaymentMethod { PaymentMethod = g.Key, Total = g.Sum(p => p.Amount), Count = g.Count() })
            .ToListAsync(ct);

        decimal cashIn = byMethod.Where(x => x.PaymentMethod == "cash").Sum(x => x.Total);

        return new OutputCashSummary
        {
            IdCashSession = session.IdCashSession,
            OpenedAt = session.OpenedAt,
            OpeningFloat = session.OpeningFloat,
            TotalSales = sales.Count,
            TotalSalesAmount = sales.Sum(x => x.TotalAmount),
            TotalReturns = returns,
            NetSales = sales.Sum(x => x.TotalAmount) - returns,
            ExpectedCash = session.OpeningFloat + cashIn,
            ByMethod = byMethod
        };
    }

    // ─── helper ──────────────────────────────────────────────────

    private static OutputSaleSelect MapSaleSelect(Sale sale) => new()
    {
        IdSale = sale.IdSale,
        FkStore = sale.FkStore,
        StoreName = sale.Store?.StoreName ?? string.Empty,
        FkCustomer = sale.FkCustomer,
        CustomerName = sale.Customer?.FullName,
        CustomerPhone = sale.Customer?.Phone,
        InvoiceNumber = sale.InvoiceNumber,
        SaleDate = sale.SaleDate,
        SubTotal = sale.SubTotal,
        TaxAmount = sale.TaxAmount,
        DiscountAmount = sale.DiscountAmount,
        TotalAmount = sale.TotalAmount,
        PaidAmount = sale.PaidAmount,
        ChangeGiven = sale.ChangeGiven,
        Status = sale.Status,
        Notes = sale.Notes,
        Lines = sale.Details.Select(d => new OutputSaleLine
        {
            IdSaleDetail = d.IdSaleDetail,
            FkProduct = d.FkProduct,
            ProductName = d.Product?.ProductName ?? string.Empty,
            ProductCode = d.Product?.ProductCode,
            FkProductVariant = d.FkProductVariant,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice,
            TaxAmount = d.TaxAmount,
            DiscountAmount = d.DiscountAmount,
            LineTotal = d.LineTotal
        }).ToList(),
        Payments = sale.Payments.Select(p => new OutputSalePayment
        {
            IdPayment = p.IdPayment,
            PaymentMethod = p.PaymentMethod,
            Amount = p.Amount,
            ReferenceNo = p.ReferenceNo,
            PaidAt = p.PaidAt
        }).ToList()
    };
}
