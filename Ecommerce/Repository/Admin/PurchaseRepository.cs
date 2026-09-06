using Ecommerce.DataAccess;
using Ecommerce.Helpers.Purchases;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.PurchaseModel;

namespace Ecommerce.Repository.Admin
{
    public class PurchaseRepository : IPurchaseInterface
    {
        private readonly EcommerceDbContext _dbContext;

        public PurchaseRepository(EcommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TableOutput<Purchase>> GetPurchaseListAsync(PurchaseListInput input)
        {
            if (input == null)
            {
                return PurchaseHelper.EmptyTableOutput(null);
            }

            try
            {
                var normalized = PurchaseHelper.NormalizeInput(input);

                var filtered = PurchaseHelper.ApplyFilters(
                    _dbContext.Purchases.AsNoTracking(),
                    normalized);

                var total = await filtered.LongCountAsync();

                var sorted = PurchaseHelper.ApplySorting(filtered, normalized.SortColumn, normalized.SortMode);

                var page = await sorted
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize)
                    .Select(p => new
                    {
                        p.ID_Purchase,
                        p.FK_Supplier,
                        p.InvoiceNumber,
                        p.PurchaseDate,
                        p.TotalAmount,
                        p.Notes,
                        p.CreatedOn,
                        p.Cancelled,
                        p.CancelledOn,
                        p.CancelledReason
                    })
                    .ToListAsync();

                var supplierIds = page.Select(x => x.FK_Supplier).Distinct().ToList();
                var supplierNames = await _dbContext.Suppliers.AsNoTracking()
                    .Where(s => supplierIds.Contains(s.ID_Supplier))
                    .Select(s => new { s.ID_Supplier, s.Name })
                    .ToListAsync();
                var nameMap = supplierNames.ToDictionary(x => x.ID_Supplier, x => x.Name);

                var rows = page.Select(p =>
                {
                    nameMap.TryGetValue(p.FK_Supplier, out var sname);
                    return new Purchase
                    {
                        ID_Purchase = p.ID_Purchase,
                        FK_Supplier = p.FK_Supplier,
                        SupplierName = sname ?? string.Empty,
                        GRNNumber = $"GRN-{p.ID_Purchase:D6}",
                        InvoiceNumber = p.InvoiceNumber ?? string.Empty,
                        PurchaseDate = p.PurchaseDate,
                        TotalAmount = p.TotalAmount,
                        PaymentStatus = "Pending",
                        Notes = p.Notes,
                        CreatedOn = p.CreatedOn,
                        Cancelled = p.Cancelled,
                        CancelledOn = p.CancelledOn,
                        CancelledReason = p.CancelledReason
                    };
                }).ToList();

                return new TableOutput<Purchase>
                {
                    TableData = rows,
                    TableSettings = new TableOutput_Settings
                    {
                        PageIndex = normalized.PageIndex,
                        PageSize = normalized.PageSize,
                        TotalCount = total
                    }
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<PurchaseDetailFull> GetPurchaseByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new PurchaseDetailFull
                    {
                        PurchaseHeader = new Purchase(),
                        PurchaseDetails = new List<PurchaseDetail>(),
                        StockBatches = new List<Stock>()
                    };
                }

                var header = await _dbContext.Purchases.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ID_Purchase == id);

                if (header == null)
                {
                    return new PurchaseDetailFull
                    {
                        PurchaseHeader = new Purchase(),
                        PurchaseDetails = new List<PurchaseDetail>(),
                        StockBatches = new List<Stock>()
                    };
                }

                var supplierName = await _dbContext.Suppliers.AsNoTracking()
                    .Where(s => s.ID_Supplier == header.FK_Supplier)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync() ?? string.Empty;

                var detailsRaw = await _dbContext.PurchaseDetails.AsNoTracking()
                    .Where(d => d.FK_Purchase == id && !d.Cancelled)
                    .OrderBy(d => d.ID_PurchaseDetail)
                    .Select(d => new
                    {
                        d.ID_PurchaseDetail,
                        d.FK_Purchase,
                        d.FK_ProductVariant,
                        d.Quantity,
                        d.PurchasePrice,
                        d.MRP,
                        d.ExpiryDate,
                        d.CreatedOn,
                        d.Cancelled
                    })
                    .ToListAsync();

                var pvIds = detailsRaw.Select(d => d.FK_ProductVariant).Distinct().ToList();

                var pvRows = await _dbContext.ProductVariants.AsNoTracking()
                    .Where(pv => pvIds.Contains(pv.ID_ProductVariant))
                    .Select(pv => new { pv.ID_ProductVariant, pv.FK_Product })
                    .ToListAsync();

                var pvToProduct = pvRows.ToDictionary(x => x.ID_ProductVariant, x => x.FK_Product);
                var productIds = pvRows.Select(x => x.FK_Product).Distinct().ToList();

                var prodNames = await _dbContext.Products.AsNoTracking()
                    .Where(p => productIds.Contains(p.ID_Product))
                    .Select(p => new { p.ID_Product, p.Name })
                    .ToListAsync();
                var prodMap = prodNames.ToDictionary(x => x.ID_Product, x => x.Name);

                // VariantAttributes string like "Color:Black, Size:XL"
                var attrRows = await (
                    from pva in _dbContext.ProductVariantAttributes.AsNoTracking()
                    join v in _dbContext.Variants.AsNoTracking() on pva.FK_Variant equals v.ID_Variant
                    join vv in _dbContext.VariantValues.AsNoTracking() on pva.FK_VariantValue equals vv.ID_VariantValue
                    where pvIds.Contains(pva.FK_ProductVariant)
                    select new { pva.FK_ProductVariant, VariantName = v.Name, ValueName = vv.Name, v.DisplayOrder }
                ).ToListAsync();

                var attrMap = attrRows
                    .GroupBy(x => x.FK_ProductVariant)
                    .ToDictionary(
                        g => g.Key,
                        g => string.Join(", ", g.OrderBy(x => x.DisplayOrder).ThenBy(x => x.VariantName)
                            .Select(x => $"{x.VariantName}:{x.ValueName}")));

                var details = detailsRaw.Select(d =>
                {
                    pvToProduct.TryGetValue(d.FK_ProductVariant, out var fkProduct);
                    prodMap.TryGetValue(fkProduct, out var pname);
                    attrMap.TryGetValue(d.FK_ProductVariant, out var attrs);

                    return new PurchaseDetail
                    {
                        ID_PurchaseDetail = d.ID_PurchaseDetail,
                        FK_Purchase = d.FK_Purchase,
                        FK_ProductVariant = d.FK_ProductVariant,
                        FK_Product = fkProduct,
                        ProductName = pname ?? string.Empty,
                        VariantAttributes = attrs ?? string.Empty,
                        Quantity = d.Quantity,
                        PurchasePrice = d.PurchasePrice,
                        MRP = d.MRP,
                        ExpiryDate = d.ExpiryDate,
                        CreatedOn = d.CreatedOn,
                        Cancelled = d.Cancelled
                    };
                }).ToList();

                var stockBatches = await _dbContext.Stock.AsNoTracking()
                    .Where(s => s.FK_PurchaseDetail > 0 && detailsRaw.Select(x => x.ID_PurchaseDetail).Contains(s.FK_PurchaseDetail))
                    .OrderBy(s => s.ID_Stock)
                    .Select(s => new Stock
                    {
                        ID_Stock = s.ID_Stock,
                        FK_PurchaseDetail = s.FK_PurchaseDetail,
                        FK_ProductVariant = s.FK_ProductVariant,
                        Quantity = s.Quantity,
                        CreatedOn = s.CreatedOn,
                        Cancelled = s.Cancelled,
                        CancelledOn = s.CancelledOn,
                        CancelledReason = s.CancelledReason
                    })
                    .ToListAsync();

                return new PurchaseDetailFull
                {
                    PurchaseHeader = new Purchase
                    {
                        ID_Purchase = header.ID_Purchase,
                        FK_Supplier = header.FK_Supplier,
                        SupplierName = supplierName,
                        GRNNumber = $"GRN-{header.ID_Purchase:D6}",
                        InvoiceNumber = header.InvoiceNumber ?? string.Empty,
                        PurchaseDate = header.PurchaseDate,
                        TotalAmount = header.TotalAmount,
                        PaymentStatus = string.IsNullOrWhiteSpace(header.PaymentStatus) ? "Pending" : header.PaymentStatus!,
                        Notes = header.Notes,
                        CreatedOn = header.CreatedOn,
                        Cancelled = header.Cancelled,
                        CancelledOn = header.CancelledOn,
                        CancelledReason = header.CancelledReason
                    },
                    PurchaseDetails = details,
                    StockBatches = stockBatches
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<CommonResponse> CreatePurchaseAsync(PurchaseUpdateInput input)
        {
            try
            {
                if (input == null)
                {
                    return Fail("Invalid request.");
                }

                if (input.FK_Supplier <= 0)
                {
                    return Fail("Supplier is required.");
                }

                if (string.IsNullOrWhiteSpace(input.PurchaseDetails))
                {
                    return Fail("At least one purchase detail is required.");
                }

                var details = System.Text.Json.JsonSerializer.Deserialize<List<PurchaseDetailVIEW>>(input.PurchaseDetails) ?? new List<PurchaseDetailVIEW>();
                if (details.Count == 0)
                {
                    return Fail("At least one purchase detail is required.");
                }

                var dupPv = details.GroupBy(d => d.FK_ProductVariant).Any(g => g.Key > 0 && g.Count() > 1);
                if (dupPv)
                {
                    return Fail("Duplicate SKU found in purchase details.");
                }

                var supplierOk = await _dbContext.Suppliers.AnyAsync(s => s.ID_Supplier == input.FK_Supplier && !s.Cancelled && s.IsActive);
                if (!supplierOk)
                {
                    return Fail("Invalid supplier.");
                }

                var pvIds = details.Select(d => d.FK_ProductVariant).Where(x => x > 0).Distinct().ToList();
                var pvCount = await _dbContext.ProductVariants.CountAsync(pv => pvIds.Contains(pv.ID_ProductVariant) && !pv.Cancelled);
                if (pvCount != pvIds.Count)
                {
                    return Fail("One or more SKUs are invalid or deleted.");
                }

                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var now = DateTime.Now;
                    var paymentStatus = string.IsNullOrWhiteSpace(input.PaymentStatus) ? "Pending" : input.PaymentStatus.Trim();
                    if (paymentStatus.Length > 30)
                    {
                        paymentStatus = paymentStatus.Substring(0, 30);
                    }

                    var purchase = new PurchaseEntity
                    {
                    FK_Supplier = input.FK_Supplier,
                        PurchaseDate = (input.PurchaseDate ?? now).Date,
                        GRNNumber = string.IsNullOrWhiteSpace(input.GRNNumber) ? null : input.GRNNumber.Trim(),
                        InvoiceNumber = string.IsNullOrWhiteSpace(input.InvoiceNumber) ? null : input.InvoiceNumber.Trim(),
                        PaymentStatus = paymentStatus,
                        Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
                        TotalAmount = 0,
                        CreatedOn = now,
                        EnterBy = input.EnterBy,
                        Cancelled = false
                    };

                    _dbContext.Purchases.Add(purchase);
                    await _dbContext.SaveChangesAsync();

                    if (string.IsNullOrWhiteSpace(purchase.GRNNumber))
                    {
                        purchase.GRNNumber = $"GRN-{purchase.ID_Purchase:D6}";
                        await _dbContext.SaveChangesAsync();
                    }

                    decimal total = 0;
                foreach (var d in details)
                    {
                        if (d.FK_ProductVariant <= 0 || d.Quantity <= 0 || d.PurchasePrice < 0)
                        {
                            await tx.RollbackAsync();
                            return Fail("Invalid purchase detail values.");
                        }

                    var det = new PurchaseDetailEntity
                        {
                        FK_Purchase = purchase.ID_Purchase,
                        FK_ProductVariant = d.FK_ProductVariant,
                            Quantity = d.Quantity,
                            PurchasePrice = d.PurchasePrice,
                        MRP = d.MRP,
                            ExpiryDate = d.ExpiryDate,
                            CreatedOn = now,
                            EnterBy = input.EnterBy,
                            Cancelled = false
                        };
                        _dbContext.PurchaseDetails.Add(det);
                        await _dbContext.SaveChangesAsync();

                        _dbContext.Stock.Add(new StockEntity
                        {
                        FK_PurchaseDetail = det.ID_PurchaseDetail,
                        FK_ProductVariant = det.FK_ProductVariant,
                            Quantity = det.Quantity,
                            CreatedOn = now,
                            EnterBy = input.EnterBy,
                            Cancelled = false
                        });

                        total += det.Quantity * det.PurchasePrice;
                    }

                    purchase.TotalAmount = total;
                    await _dbContext.SaveChangesAsync();

                    await tx.CommitAsync();
                return Ok(purchase.ID_Purchase, "Purchase created successfully.");
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<CommonResponse> UpdatePurchaseAsync(PurchaseUpdateInput input)
        {
            try
            {
                if (input == null)
                {
                    return Fail("Invalid request.");
                }

                if (input.ID_Purchase <= 0)
                {
                    return Fail("Invalid purchase ID.");
                }

                if (input.FK_Supplier <= 0)
                {
                    return Fail("Supplier is required.");
                }

                if (string.IsNullOrWhiteSpace(input.PurchaseDetails))
                {
                    return Fail("At least one purchase detail is required.");
                }

                var details = System.Text.Json.JsonSerializer.Deserialize<List<PurchaseDetailVIEW>>(input.PurchaseDetails) ?? new List<PurchaseDetailVIEW>();
                if (details.Count == 0)
                {
                    return Fail("At least one purchase detail is required.");
                }

                var dupPv = details.GroupBy(d => d.FK_ProductVariant).Any(g => g.Key > 0 && g.Count() > 1);
                if (dupPv)
                {
                    return Fail("Duplicate SKU found in purchase details.");
                }

                var purchase = await _dbContext.Purchases.FirstOrDefaultAsync(p => p.ID_Purchase == input.ID_Purchase);
                if (purchase == null)
                {
                    return Fail("Purchase not found.");
                }

                if (purchase.Cancelled)
                {
                    return Fail("This purchase is deleted and cannot be edited.");
                }

                var supplierOk = await _dbContext.Suppliers.AnyAsync(s => s.ID_Supplier == input.FK_Supplier && !s.Cancelled && s.IsActive);
                if (!supplierOk)
                {
                    return Fail("Invalid supplier.");
                }

                var pvIds = details.Select(d => d.FK_ProductVariant).Where(x => x > 0).Distinct().ToList();
                var pvCount = await _dbContext.ProductVariants.CountAsync(pv => pvIds.Contains(pv.ID_ProductVariant) && !pv.Cancelled);
                if (pvCount != pvIds.Count)
                {
                    return Fail("One or more SKUs are invalid or deleted.");
                }

                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var now = DateTime.Now;

                    // Soft-cancel existing active details + stock batches
                    var existingDetails = await _dbContext.PurchaseDetails
                        .Where(d => d.FK_Purchase == purchase.ID_Purchase && !d.Cancelled)
                        .ToListAsync();

                    foreach (var d in existingDetails)
                    {
                        d.Cancelled = true;
                        d.CancelledOn = now;
                        d.CancelledReason = "Updated";
                        d.CancelledBy = input.EnterBy;
                    }

                    var existingDetailIds = existingDetails.Select(d => d.ID_PurchaseDetail).ToList();
                    var existingStock = await _dbContext.Stock
                        .Where(s => existingDetailIds.Contains(s.FK_PurchaseDetail) && !s.Cancelled)
                        .ToListAsync();

                    foreach (var s in existingStock)
                    {
                        s.Cancelled = true;
                        s.CancelledOn = now;
                        s.CancelledReason = "Updated";
                        s.CancelledBy = input.EnterBy;
                    }

                    // Update header
                    purchase.FK_Supplier = input.FK_Supplier;
                    purchase.PurchaseDate = (input.PurchaseDate ?? now).Date;
                    purchase.GRNNumber = string.IsNullOrWhiteSpace(input.GRNNumber) ? purchase.GRNNumber : input.GRNNumber.Trim();
                    purchase.InvoiceNumber = string.IsNullOrWhiteSpace(input.InvoiceNumber) ? null : input.InvoiceNumber.Trim();
                    purchase.PaymentStatus = string.IsNullOrWhiteSpace(input.PaymentStatus) ? (purchase.PaymentStatus ?? "Pending") : input.PaymentStatus.Trim();
                    purchase.Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim();

                    if (string.IsNullOrWhiteSpace(purchase.GRNNumber))
                    {
                        purchase.GRNNumber = $"GRN-{purchase.ID_Purchase:D6}";
                    }

                    // Insert new details + new stock batches
                    decimal total = 0;
                    foreach (var d in details)
                    {
                        if (d.FK_ProductVariant <= 0 || d.Quantity <= 0 || d.PurchasePrice < 0)
                        {
                            await tx.RollbackAsync();
                            return Fail("Invalid purchase detail values.");
                        }

                        var det = new PurchaseDetailEntity
                        {
                            FK_Purchase = purchase.ID_Purchase,
                            FK_ProductVariant = d.FK_ProductVariant,
                            Quantity = d.Quantity,
                            PurchasePrice = d.PurchasePrice,
                            MRP = d.MRP,
                            ExpiryDate = d.ExpiryDate,
                            CreatedOn = now,
                            EnterBy = input.EnterBy,
                            Cancelled = false
                        };
                        _dbContext.PurchaseDetails.Add(det);
                        await _dbContext.SaveChangesAsync();

                        _dbContext.Stock.Add(new StockEntity
                        {
                            FK_PurchaseDetail = det.ID_PurchaseDetail,
                            FK_ProductVariant = det.FK_ProductVariant,
                            Quantity = det.Quantity,
                            CreatedOn = now,
                            EnterBy = input.EnterBy,
                            Cancelled = false
                        });

                        total += det.Quantity * det.PurchasePrice;
                    }

                    purchase.TotalAmount = total;
                    await _dbContext.SaveChangesAsync();
                    await tx.CommitAsync();

                    return Ok(purchase.ID_Purchase, "Purchase updated successfully.");
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<CommonResponse> DeletePurchaseAsync(PurchaseDeleteInput input)
        {
            try
            {
                if (input == null)
                {
                    return Fail("Invalid purchase ID.");
                }

                if (input.ID_Purchase <= 0)
                {
                    return Fail("Invalid purchase ID.");
                }

                var purchase = await _dbContext.Purchases.FirstOrDefaultAsync(p => p.ID_Purchase == input.ID_Purchase);
                if (purchase == null)
                {
                    return Fail("Purchase not found.");
                }

                if (purchase.Cancelled)
                {
                    return Fail("This purchase is already deleted.");
                }

                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var now = DateTime.Now;

                    purchase.Cancelled = true;
                    purchase.CancelledOn = now;
                    purchase.CancelledReason = input.CancelledReason;
                    purchase.CancelledBy = input.EnterBy;

                    var details = await _dbContext.PurchaseDetails
                        .Where(d => d.FK_Purchase == purchase.ID_Purchase && !d.Cancelled)
                        .ToListAsync();

                    foreach (var d in details)
                    {
                        d.Cancelled = true;
                        d.CancelledOn = now;
                        d.CancelledReason = input.CancelledReason;
                        d.CancelledBy = input.EnterBy;
                    }

                    var detailIds = details.Select(d => d.ID_PurchaseDetail).ToList();
                    var stock = await _dbContext.Stock
                        .Where(s => detailIds.Contains(s.FK_PurchaseDetail) && !s.Cancelled)
                        .ToListAsync();

                    foreach (var s in stock)
                    {
                        s.Cancelled = true;
                        s.CancelledOn = now;
                        s.CancelledReason = input.CancelledReason;
                        s.CancelledBy = input.EnterBy;
                    }

                    await _dbContext.SaveChangesAsync();
                    await tx.CommitAsync();

                    return Ok(input.ID_Purchase, "Purchase deleted successfully.");
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch
            {
                throw;
            }
        }

        private static CommonResponse Ok(long responseCode, string message) =>
            new()
            {
                ResponseCode = responseCode,
                StatusCode = true,
                ResponseMsg = message
            };

        private static CommonResponse Fail(string message) =>
            new()
            {
                ResponseCode = -1,
                StatusCode = false,
                ResponseMsg = message
            };
    }
}
