using Ecommerce.DataAccess;
using Ecommerce.Helpers.Sales;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.SaleModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class SaleRepository : ISaleInterface
    {
        private readonly EcommerceDbContext _db;

        public SaleRepository(EcommerceDbContext db)
        {
            _db = db;
        }

        public async Task<TableOutput<Sale>> GetSaleListAsync(SaleListInput input)
        {
            input ??= new SaleListInput();
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var sortDesc = !string.Equals(input.SortMode, "ASC", StringComparison.OrdinalIgnoreCase);
            var search = input.SearchText?.Trim().ToLowerInvariant();
            var pay = input.PaymentMethod?.Trim();

            var query = _db.Sales.AsNoTracking().Where(s => !s.Cancelled);

            if (input.FromDate.HasValue)
            {
                var from = input.FromDate.Value.Date;
                query = query.Where(s => s.SaleDate >= from);
            }

            if (input.ToDate.HasValue)
            {
                var to = input.ToDate.Value.Date;
                query = query.Where(s => s.SaleDate <= to);
            }

            if (!string.IsNullOrWhiteSpace(pay))
            {
                query = query.Where(s => s.PaymentMethod == pay);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    (s.InvoiceNumber ?? string.Empty).ToLower().Contains(search) ||
                    (s.CustomerName ?? string.Empty).ToLower().Contains(search) ||
                    (s.CustomerPhone ?? string.Empty).ToLower().Contains(search));
            }

            var total = await query.LongCountAsync();

            query = input.SortColumn switch
            {
                2 => sortDesc ? query.OrderByDescending(s => s.TotalAmount) : query.OrderBy(s => s.TotalAmount),
                3 => sortDesc ? query.OrderByDescending(s => s.InvoiceNumber) : query.OrderBy(s => s.InvoiceNumber),
                1 => sortDesc ? query.OrderByDescending(s => s.SaleDate) : query.OrderBy(s => s.SaleDate),
                _ => sortDesc ? query.OrderByDescending(s => s.ID_Sale) : query.OrderBy(s => s.ID_Sale)
            };

            var page = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var saleIds = page.Select(s => s.ID_Sale).ToList();
            var returnedSaleIds = await _db.SalesReturns.AsNoTracking()
                .Where(r => saleIds.Contains(r.FK_Sale) && !r.Cancelled)
                .Select(r => r.FK_Sale)
                .Distinct()
                .ToListAsync();
            var returnedSet = returnedSaleIds.ToHashSet();

            var rows = page.Select(s => MapSale(s, returnedSet.Contains(s.ID_Sale))).ToList();

            return new TableOutput<Sale>
            {
                TableData = rows,
                TableSettings = new TableOutput_Settings
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalCount = total
                }
            };
        }

        public async Task<SaleDetailFull> GetSaleByIdAsync(int id)
        {
            if (id <= 0)
            {
                return new SaleDetailFull();
            }

            var header = await _db.Sales.AsNoTracking().FirstOrDefaultAsync(s => s.ID_Sale == id);
            if (header == null)
            {
                return new SaleDetailFull();
            }

            var hasReturns = await _db.SalesReturns.AsNoTracking()
                .AnyAsync(r => r.FK_Sale == id && !r.Cancelled);

            var details = await LoadDetailsAsync(id);

            return new SaleDetailFull
            {
                SaleHeader = MapSale(header, hasReturns),
                SaleDetails = details
            };
        }

        public async Task<List<SaleSkuOption>> GetSkuOptionsAsync(int productId)
        {
            var variants = await _db.ProductVariants.AsNoTracking()
                .Where(pv => pv.FK_Product == productId && !pv.Cancelled && pv.IsActive)
                .OrderBy(pv => pv.VariantLabel)
                .ToListAsync();

            if (variants.Count == 0)
            {
                return new List<SaleSkuOption>();
            }

            var ids = variants.Select(v => v.ID_ProductVariant).ToList();
            var stockRows = await _db.Stock.AsNoTracking()
                .Where(s => ids.Contains(s.FK_ProductVariant) && !s.Cancelled)
                .GroupBy(s => s.FK_ProductVariant)
                .Select(g => new { Id = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();
            var stockMap = stockRows.ToDictionary(x => x.Id, x => Math.Max(0, x.Qty));

            var productName = await _db.Products.AsNoTracking()
                .Where(p => p.ID_Product == productId)
                .Select(p => p.Name)
                .FirstOrDefaultAsync() ?? string.Empty;

            return variants.Select(v => new SaleSkuOption
            {
                FK_Product = v.FK_Product,
                ID_ProductVariant = v.ID_ProductVariant,
                ProductName = productName,
                VariantLabel = v.VariantLabel,
                SKU = v.SKU,
                SellingPrice = v.SellingPrice,
                MRP = v.MRP,
                AvailableQty = stockMap.TryGetValue(v.ID_ProductVariant, out var qty) ? qty : 0
            }).ToList();
        }

        public async Task<CommonResponse> CreateSaleAsync(SaleUpdateInput input)
        {
            var check = ValidateHeader(input);
            if (check != null)
            {
                return Fail(check);
            }

            var details = NormalizeDetails(input.SaleDetails);
            if (details.Count == 0)
            {
                return Fail("At least one sale item is required.");
            }

            var skuError = await ValidateSkusAsync(details);
            if (skuError != null)
            {
                return Fail(skuError);
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                var sale = new SaleEntity
                {
                    SaleDate = input.SaleDate.Date,
                    CustomerName = string.IsNullOrWhiteSpace(input.CustomerName) ? "Walk-in" : input.CustomerName.Trim(),
                    CustomerPhone = string.IsNullOrWhiteSpace(input.CustomerPhone) ? null : input.CustomerPhone.Trim(),
                    PaymentMethod = NormalizePayment(input.PaymentMethod),
                    InvoiceNumber = string.IsNullOrWhiteSpace(input.InvoiceNumber) ? null : input.InvoiceNumber.Trim(),
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
                    TotalAmount = 0,
                    CreatedOn = now,
                    EnterBy = input.EnterBy,
                    Cancelled = false
                };

                _db.Sales.Add(sale);
                await _db.SaveChangesAsync();

                if (string.IsNullOrWhiteSpace(sale.InvoiceNumber))
                {
                    sale.InvoiceNumber = $"SALE-{sale.ID_Sale:D6}";
                }

                var deductError = await WriteLinesAndDeductAsync(sale, details, input.EnterBy, now);
                if (deductError != null)
                {
                    await tx.RollbackAsync();
                    return Fail(deductError);
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(sale.ID_Sale, "Sale saved.");
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<CommonResponse> UpdateSaleAsync(SaleUpdateInput input)
        {
            if (input.ID_Sale <= 0)
            {
                return Fail("Invalid sale.");
            }

            var check = ValidateHeader(input);
            if (check != null)
            {
                return Fail(check);
            }

            var details = NormalizeDetails(input.SaleDetails);
            if (details.Count == 0)
            {
                return Fail("At least one sale item is required.");
            }

            var sale = await _db.Sales.FirstOrDefaultAsync(s => s.ID_Sale == input.ID_Sale);
            if (sale == null)
            {
                return Fail("Sale not found.");
            }

            if (sale.Cancelled)
            {
                return Fail("This sale is deleted and cannot be edited.");
            }

            var hasReturns = await _db.SalesReturns.AnyAsync(r => r.FK_Sale == sale.ID_Sale && !r.Cancelled);
            if (hasReturns)
            {
                return Fail("This sale has a return. Cancel the return before editing.");
            }

            var skuError = await ValidateSkusAsync(details);
            if (skuError != null)
            {
                return Fail(skuError);
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                var existing = await _db.SaleDetails
                    .Where(d => d.FK_Sale == sale.ID_Sale && !d.Cancelled)
                    .ToListAsync();

                foreach (var line in existing)
                {
                    await StockLedgerHelper.RestoreAsync(_db, line.FK_ProductVariant, line.Quantity, input.EnterBy, now);
                    line.Cancelled = true;
                    line.CancelledOn = now;
                    line.CancelledReason = "Updated";
                    line.CancelledBy = input.EnterBy;
                }

                await _db.SaveChangesAsync();

                sale.SaleDate = input.SaleDate.Date;
                sale.CustomerName = string.IsNullOrWhiteSpace(input.CustomerName) ? "Walk-in" : input.CustomerName.Trim();
                sale.CustomerPhone = string.IsNullOrWhiteSpace(input.CustomerPhone) ? null : input.CustomerPhone.Trim();
                sale.PaymentMethod = NormalizePayment(input.PaymentMethod);
                sale.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();
                if (!string.IsNullOrWhiteSpace(input.InvoiceNumber))
                {
                    sale.InvoiceNumber = input.InvoiceNumber.Trim();
                }
                else if (string.IsNullOrWhiteSpace(sale.InvoiceNumber))
                {
                    sale.InvoiceNumber = $"SALE-{sale.ID_Sale:D6}";
                }

                var deductError = await WriteLinesAndDeductAsync(sale, details, input.EnterBy, now);
                if (deductError != null)
                {
                    await tx.RollbackAsync();
                    return Fail(deductError);
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(sale.ID_Sale, "Sale updated.");
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<CommonResponse> DeleteSaleAsync(SaleDeleteInput input)
        {
            if (input == null || input.ID_Sale <= 0)
            {
                return Fail("Invalid sale.");
            }

            var sale = await _db.Sales.FirstOrDefaultAsync(s => s.ID_Sale == input.ID_Sale);
            if (sale == null)
            {
                return Fail("Sale not found.");
            }

            if (sale.Cancelled)
            {
                return Fail("This sale is already deleted.");
            }

            var hasReturns = await _db.SalesReturns.AnyAsync(r => r.FK_Sale == sale.ID_Sale && !r.Cancelled);
            if (hasReturns)
            {
                return Fail("This sale has a return. Cancel the return first.");
            }

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                sale.Cancelled = true;
                sale.CancelledOn = now;
                sale.CancelledReason = input.CancelledReason;
                sale.CancelledBy = input.EnterBy;

                var details = await _db.SaleDetails
                    .Where(d => d.FK_Sale == sale.ID_Sale && !d.Cancelled)
                    .ToListAsync();

                foreach (var line in details)
                {
                    await StockLedgerHelper.RestoreAsync(_db, line.FK_ProductVariant, line.Quantity, input.EnterBy, now);
                    line.Cancelled = true;
                    line.CancelledOn = now;
                    line.CancelledReason = input.CancelledReason;
                    line.CancelledBy = input.EnterBy;
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(sale.ID_Sale, "Sale deleted. Stock restored.");
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private async Task<List<SaleDetail>> LoadDetailsAsync(int saleId)
        {
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
                var fkProduct = pv?.FK_Product ?? 0;
                prodMap.TryGetValue(fkProduct, out var pname);
                returnedMap.TryGetValue(d.ID_SaleDetail, out var retQty);
                return new SaleDetail
                {
                    ID_SaleDetail = d.ID_SaleDetail,
                    FK_Sale = d.FK_Sale,
                    FK_ProductVariant = d.FK_ProductVariant,
                    FK_Product = fkProduct,
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

        private async Task<string?> WriteLinesAndDeductAsync(
            SaleEntity sale,
            List<SaleDetailVIEW> details,
            int enterBy,
            DateTime now)
        {
            decimal total = 0;
            foreach (var d in details)
            {
                var deductError = await StockLedgerHelper.DeductFifoAsync(_db, d.FK_ProductVariant, d.Quantity);
                if (deductError != null)
                {
                    return deductError;
                }

                _db.SaleDetails.Add(new SaleDetailEntity
                {
                    FK_Sale = sale.ID_Sale,
                    FK_ProductVariant = d.FK_ProductVariant,
                    Quantity = d.Quantity,
                    SellingPrice = d.SellingPrice,
                    MRP = d.MRP,
                    CreatedOn = now,
                    EnterBy = enterBy,
                    Cancelled = false
                });

                total += d.Quantity * d.SellingPrice;
            }

            sale.TotalAmount = total;
            return null;
        }

        private async Task<string?> ValidateSkusAsync(List<SaleDetailVIEW> details)
        {
            var ids = details.Select(d => d.FK_ProductVariant).Distinct().ToList();
            var count = await _db.ProductVariants.CountAsync(pv => ids.Contains(pv.ID_ProductVariant) && !pv.Cancelled);
            return count == ids.Count ? null : "One or more SKUs are invalid or deleted.";
        }

        private static List<SaleDetailVIEW> NormalizeDetails(List<SaleDetailVIEW>? details)
        {
            return (details ?? new List<SaleDetailVIEW>())
                .Where(d => d.FK_ProductVariant > 0 && d.Quantity > 0 && d.SellingPrice >= 0)
                .ToList();
        }

        private static string? ValidateHeader(SaleUpdateInput input)
        {
            if (input == null)
            {
                return "Invalid request.";
            }

            if (input.SaleDate == default)
            {
                return "Sale date is required.";
            }

            return null;
        }

        private static string NormalizePayment(string? value)
        {
            var pay = string.IsNullOrWhiteSpace(value) ? "Cash" : value.Trim();
            return pay.Length > 30 ? pay[..30] : pay;
        }

        private static Sale MapSale(SaleEntity s, bool hasReturns) => new()
        {
            ID_Sale = s.ID_Sale,
            InvoiceNumber = s.InvoiceNumber ?? $"SALE-{s.ID_Sale:D6}",
            SaleDate = s.SaleDate,
            CustomerName = s.CustomerName ?? "Walk-in",
            CustomerPhone = s.CustomerPhone ?? string.Empty,
            PaymentMethod = s.PaymentMethod,
            TotalAmount = s.TotalAmount,
            Notes = s.Notes,
            CreatedOn = s.CreatedOn,
            Cancelled = s.Cancelled,
            CancelledOn = s.CancelledOn,
            CancelledReason = s.CancelledReason,
            HasReturns = hasReturns
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
