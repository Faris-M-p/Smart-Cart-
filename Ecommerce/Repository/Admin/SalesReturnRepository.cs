using Ecommerce.DataAccess;
using Ecommerce.Helpers.Sales;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.SaleModel;
using static Ecommerce.Models.Admin.SalesReturnModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class SalesReturnRepository : ISalesReturnInterface
    {
        private readonly EcommerceDbContext _db;

        public SalesReturnRepository(EcommerceDbContext db)
        {
            _db = db;
        }

        public async Task<TableOutput<SalesReturn>> GetSalesReturnListAsync(SalesReturnListInput input)
        {
            input ??= new SalesReturnListInput();
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var sortDesc = !string.Equals(input.SortMode, "ASC", StringComparison.OrdinalIgnoreCase);
            var search = input.SearchText?.Trim().ToLowerInvariant();

            var query =
                from r in _db.SalesReturns.AsNoTracking()
                join s in _db.Sales.AsNoTracking() on r.FK_Sale equals s.ID_Sale
                where !r.Cancelled
                select new { r, s };

            if (input.FromDate.HasValue)
            {
                var from = input.FromDate.Value.Date;
                query = query.Where(x => x.r.ReturnDate >= from);
            }

            if (input.ToDate.HasValue)
            {
                var to = input.ToDate.Value.Date;
                query = query.Where(x => x.r.ReturnDate <= to);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.r.InvoiceNumber ?? string.Empty).ToLower().Contains(search) ||
                    (x.s.InvoiceNumber ?? string.Empty).ToLower().Contains(search) ||
                    (x.s.CustomerName ?? string.Empty).ToLower().Contains(search));
            }

            var total = await query.LongCountAsync();

            query = input.SortColumn switch
            {
                2 => sortDesc ? query.OrderByDescending(x => x.r.TotalAmount) : query.OrderBy(x => x.r.TotalAmount),
                3 => sortDesc ? query.OrderByDescending(x => x.r.InvoiceNumber) : query.OrderBy(x => x.r.InvoiceNumber),
                1 => sortDesc ? query.OrderByDescending(x => x.r.ReturnDate) : query.OrderBy(x => x.r.ReturnDate),
                _ => sortDesc ? query.OrderByDescending(x => x.r.ID_SalesReturn) : query.OrderBy(x => x.r.ID_SalesReturn)
            };

            var page = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new TableOutput<SalesReturn>
            {
                TableData = page.Select(x => MapReturn(x.r, x.s)).ToList(),
                TableSettings = new TableOutput_Settings
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalCount = total
                }
            };
        }

        public async Task<SalesReturnDetailFull> GetSalesReturnByIdAsync(int id)
        {
            if (id <= 0)
            {
                return new SalesReturnDetailFull();
            }

            var header = await _db.SalesReturns.AsNoTracking()
                .FirstOrDefaultAsync(r => r.ID_SalesReturn == id);
            if (header == null)
            {
                return new SalesReturnDetailFull();
            }

            var sale = await _db.Sales.AsNoTracking()
                .FirstOrDefaultAsync(s => s.ID_Sale == header.FK_Sale);

            var lines = await _db.SalesReturnDetails.AsNoTracking()
                .Where(d => d.FK_SalesReturn == id && !d.Cancelled)
                .OrderBy(d => d.ID_SalesReturnDetail)
                .ToListAsync();

            var pvIds = lines.Select(d => d.FK_ProductVariant).Distinct().ToList();
            var variants = await _db.ProductVariants.AsNoTracking()
                .Where(pv => pvIds.Contains(pv.ID_ProductVariant))
                .Select(pv => new { pv.ID_ProductVariant, pv.FK_Product, pv.SKU, pv.VariantLabel })
                .ToListAsync();
            var pvMap = variants.ToDictionary(x => x.ID_ProductVariant);
            var productIds = variants.Select(x => x.FK_Product).Distinct().ToList();
            var products = await _db.Products.AsNoTracking()
                .Where(p => productIds.Contains(p.ID_Product))
                .Select(p => new { p.ID_Product, p.Name })
                .ToListAsync();
            var prodMap = products.ToDictionary(x => x.ID_Product, x => x.Name);

            return new SalesReturnDetailFull
            {
                ReturnHeader = MapReturn(header, sale),
                ReturnDetails = lines.Select(d =>
                {
                    pvMap.TryGetValue(d.FK_ProductVariant, out var pv);
                    prodMap.TryGetValue(pv?.FK_Product ?? 0, out var pname);
                    return new SalesReturnDetail
                    {
                        ID_SalesReturnDetail = d.ID_SalesReturnDetail,
                        FK_SalesReturn = d.FK_SalesReturn,
                        FK_SaleDetail = d.FK_SaleDetail,
                        FK_ProductVariant = d.FK_ProductVariant,
                        ProductName = pname ?? string.Empty,
                        VariantLabel = pv?.VariantLabel ?? string.Empty,
                        SKU = pv?.SKU ?? string.Empty,
                        Quantity = d.Quantity,
                        SellingPrice = d.SellingPrice,
                        LineTotal = d.Quantity * d.SellingPrice
                    };
                }).ToList()
            };
        }

        public async Task<List<SaleLookup>> GetSaleLookupsAsync()
        {
            var list = await _db.Sales.AsNoTracking()
                .Where(s => !s.Cancelled)
                .OrderByDescending(s => s.ID_Sale)
                .Take(200)
                .Select(s => new SaleLookup
                {
                    ID_Sale = s.ID_Sale,
                    InvoiceNumber = s.InvoiceNumber ?? string.Empty,
                    CustomerName = s.CustomerName ?? "Walk-in",
                    SaleDate = s.SaleDate,
                    TotalAmount = s.TotalAmount
                })
                .ToListAsync();

            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item.InvoiceNumber))
                {
                    item.InvoiceNumber = $"SALE-{item.ID_Sale:D6}";
                }
            }

            return list;
        }

        public async Task<List<SaleDetail>> GetReturnableLinesAsync(int saleId)
        {
            if (saleId <= 0)
            {
                return new List<SaleDetail>();
            }

            var sale = await _db.Sales.AsNoTracking()
                .FirstOrDefaultAsync(s => s.ID_Sale == saleId && !s.Cancelled);
            if (sale == null)
            {
                return new List<SaleDetail>();
            }

            var detailsRaw = await _db.SaleDetails.AsNoTracking()
                .Where(d => d.FK_Sale == saleId && !d.Cancelled)
                .OrderBy(d => d.ID_SaleDetail)
                .ToListAsync();

            var returned = await (
                from rd in _db.SalesReturnDetails.AsNoTracking()
                join r in _db.SalesReturns.AsNoTracking() on rd.FK_SalesReturn equals r.ID_SalesReturn
                where r.FK_Sale == saleId && !r.Cancelled && !rd.Cancelled
                group rd by rd.FK_SaleDetail into g
                select new { Id = g.Key, Qty = g.Sum(x => x.Quantity) }
            ).ToListAsync();
            var returnedMap = returned.ToDictionary(x => x.Id, x => x.Qty);

            var pvIds = detailsRaw.Select(d => d.FK_ProductVariant).Distinct().ToList();
            var variants = await _db.ProductVariants.AsNoTracking()
                .Where(pv => pvIds.Contains(pv.ID_ProductVariant))
                .Select(pv => new { pv.ID_ProductVariant, pv.FK_Product, pv.SKU, pv.VariantLabel })
                .ToListAsync();
            var pvMap = variants.ToDictionary(x => x.ID_ProductVariant);
            var productIds = variants.Select(x => x.FK_Product).Distinct().ToList();
            var products = await _db.Products.AsNoTracking()
                .Where(p => productIds.Contains(p.ID_Product))
                .Select(p => new { p.ID_Product, p.Name })
                .ToListAsync();
            var prodMap = products.ToDictionary(x => x.ID_Product, x => x.Name);

            return detailsRaw.Select(d =>
            {
                pvMap.TryGetValue(d.FK_ProductVariant, out var pv);
                prodMap.TryGetValue(pv?.FK_Product ?? 0, out var pname);
                returnedMap.TryGetValue(d.ID_SaleDetail, out var retQty);
                return new SaleDetail
                {
                    ID_SaleDetail = d.ID_SaleDetail,
                    FK_Sale = d.FK_Sale,
                    FK_ProductVariant = d.FK_ProductVariant,
                    FK_Product = pv?.FK_Product ?? 0,
                    ProductName = pname ?? string.Empty,
                    VariantLabel = pv?.VariantLabel ?? string.Empty,
                    SKU = pv?.SKU ?? string.Empty,
                    Quantity = d.Quantity,
                    SellingPrice = d.SellingPrice,
                    MRP = d.MRP,
                    LineTotal = d.Quantity * d.SellingPrice,
                    ReturnedQty = retQty,
                    ReturnableQty = Math.Max(0, d.Quantity - retQty)
                };
            }).ToList();
        }

        public async Task<CommonResponse> CreateSalesReturnAsync(SalesReturnUpdateInput input)
        {
            if (input == null || input.FK_Sale <= 0)
            {
                return Fail("Select a sale to return.");
            }

            var lines = (input.ReturnDetails ?? new List<SalesReturnDetailVIEW>())
                .Where(d => d.FK_SaleDetail > 0 && d.Quantity > 0)
                .ToList();
            if (lines.Count == 0)
            {
                return Fail("Enter at least one return quantity.");
            }

            var sale = await _db.Sales.FirstOrDefaultAsync(s => s.ID_Sale == input.FK_Sale);
            if (sale == null || sale.Cancelled)
            {
                return Fail("Sale not found or already deleted.");
            }

            var saleLines = await _db.SaleDetails
                .Where(d => d.FK_Sale == sale.ID_Sale && !d.Cancelled)
                .ToListAsync();
            var saleLineMap = saleLines.ToDictionary(d => d.ID_SaleDetail);

            var alreadyReturned = await (
                from rd in _db.SalesReturnDetails
                join r in _db.SalesReturns on rd.FK_SalesReturn equals r.ID_SalesReturn
                where r.FK_Sale == sale.ID_Sale && !r.Cancelled && !rd.Cancelled
                group rd by rd.FK_SaleDetail into g
                select new { Id = g.Key, Qty = g.Sum(x => x.Quantity) }
            ).ToListAsync();
            var returnedMap = alreadyReturned.ToDictionary(x => x.Id, x => x.Qty);

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                var ret = new SalesReturnEntity
                {
                    FK_Sale = sale.ID_Sale,
                    ReturnDate = input.ReturnDate == default ? now.Date : input.ReturnDate.Date,
                    Reason = string.IsNullOrWhiteSpace(input.Reason) ? null : input.Reason.Trim(),
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
                    TotalAmount = 0,
                    CreatedOn = now,
                    EnterBy = input.EnterBy,
                    Cancelled = false
                };
                _db.SalesReturns.Add(ret);
                await _db.SaveChangesAsync();
                ret.InvoiceNumber = $"SRET-{ret.ID_SalesReturn:D6}";

                decimal total = 0;
                foreach (var line in lines)
                {
                    if (!saleLineMap.TryGetValue(line.FK_SaleDetail, out var sold))
                    {
                        await tx.RollbackAsync();
                        return Fail("A return line does not belong to this sale.");
                    }

                    returnedMap.TryGetValue(line.FK_SaleDetail, out var used);
                    var remain = sold.Quantity - used;
                    if (line.Quantity > remain)
                    {
                        await tx.RollbackAsync();
                        return Fail($"Return qty for {sold.FK_ProductVariant} exceeds remaining {remain}.");
                    }

                    _db.SalesReturnDetails.Add(new SalesReturnDetailEntity
                    {
                        FK_SalesReturn = ret.ID_SalesReturn,
                        FK_SaleDetail = sold.ID_SaleDetail,
                        FK_ProductVariant = sold.FK_ProductVariant,
                        Quantity = line.Quantity,
                        SellingPrice = sold.SellingPrice,
                        CreatedOn = now,
                        EnterBy = input.EnterBy,
                        Cancelled = false
                    });

                    await StockLedgerHelper.RestoreAsync(_db, sold.FK_ProductVariant, line.Quantity, input.EnterBy, now);
                    total += line.Quantity * sold.SellingPrice;
                }

                ret.TotalAmount = total;
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(ret.ID_SalesReturn, "Sales return saved. Stock restored.");
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<CommonResponse> DeleteSalesReturnAsync(SalesReturnDeleteInput input)
        {
            if (input == null || input.ID_SalesReturn <= 0)
            {
                return Fail("Invalid sales return.");
            }

            var ret = await _db.SalesReturns.FirstOrDefaultAsync(r => r.ID_SalesReturn == input.ID_SalesReturn);
            if (ret == null)
            {
                return Fail("Sales return not found.");
            }

            if (ret.Cancelled)
            {
                return Fail("This sales return is already deleted.");
            }

            var lines = await _db.SalesReturnDetails
                .Where(d => d.FK_SalesReturn == ret.ID_SalesReturn && !d.Cancelled)
                .ToListAsync();

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                foreach (var line in lines)
                {
                    var deductError = await StockLedgerHelper.DeductFifoAsync(_db, line.FK_ProductVariant, line.Quantity);
                    if (deductError != null)
                    {
                        await tx.RollbackAsync();
                        return Fail("Cannot cancel this return: " + deductError);
                    }

                    line.Cancelled = true;
                    line.CancelledOn = now;
                    line.CancelledReason = input.CancelledReason;
                    line.CancelledBy = input.EnterBy;
                }

                ret.Cancelled = true;
                ret.CancelledOn = now;
                ret.CancelledReason = input.CancelledReason;
                ret.CancelledBy = input.EnterBy;

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(ret.ID_SalesReturn, "Sales return deleted. Stock deducted again.");
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static SalesReturn MapReturn(SalesReturnEntity r, SaleEntity? s) => new()
        {
            ID_SalesReturn = r.ID_SalesReturn,
            FK_Sale = r.FK_Sale,
            InvoiceNumber = r.InvoiceNumber ?? $"SRET-{r.ID_SalesReturn:D6}",
            SaleInvoiceNumber = s?.InvoiceNumber ?? $"SALE-{r.FK_Sale:D6}",
            CustomerName = s?.CustomerName ?? "Walk-in",
            ReturnDate = r.ReturnDate,
            TotalAmount = r.TotalAmount,
            Reason = r.Reason,
            Notes = r.Notes,
            CreatedOn = r.CreatedOn,
            Cancelled = r.Cancelled,
            CancelledOn = r.CancelledOn,
            CancelledReason = r.CancelledReason
        };

        private static CommonResponse Ok(long code, string message) => new()
        {
            ResponseCode = code,
            StatusCode = true,
            ResponseMsg = message
        };

        private static CommonResponse Fail(string message) => new()
        {
            ResponseCode = -1,
            StatusCode = false,
            ResponseMsg = message
        };
    }
}
