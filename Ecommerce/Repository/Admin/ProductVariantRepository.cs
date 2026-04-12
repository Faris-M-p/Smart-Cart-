using Ecommerce.DataAccess;
using Ecommerce.Helpers.ProductVariants;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.ProductVariantModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class ProductVariantRepository : IProductVariantInterface
    {
        private readonly EcommerceDbContext _dbContext;

        public ProductVariantRepository(EcommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TableOutput<ProductVariant>> GetProductVariantListAsync(ProductVariantListInput input)
        {
            if (input == null)
            {
                return ProductVariantHelper.EmptyTableOutput(null);
            }

            if (input.FK_Product <= 0)
            {
                return ProductVariantHelper.EmptyTableOutput(input);
            }

            try
            {
                var normalized = ProductVariantHelper.NormalizeInput(input);

                var filteredQuery = ProductVariantHelper.ApplyFilters(
                    _dbContext.ProductVariants.AsNoTracking(),
                    input.FK_Product,
                    normalized);

                if (normalized.FilterVariantIds.Count > 0)
                {
                    var fids = normalized.FilterVariantIds;
                    filteredQuery = filteredQuery.Where(pv =>
                        _dbContext.ProductVariantAttributes.AsNoTracking().Any(pva =>
                            pva.FkProductVariant == pv.IdProductVariant &&
                            fids.Contains(pva.FkVariant)));
                }

                if (normalized.FilterVariantValueIds.Count > 0)
                {
                    var vids = normalized.FilterVariantValueIds;
                    filteredQuery = filteredQuery.Where(pv =>
                        _dbContext.ProductVariantAttributes.AsNoTracking().Any(pva =>
                            pva.FkProductVariant == pv.IdProductVariant &&
                            vids.Contains(pva.FkVariantValue)));
                }

                var totalCount = await filteredQuery.LongCountAsync();

                var productName = await _dbContext.Products.AsNoTracking()
                    .Where(p => p.IdProduct == input.FK_Product && !p.Cancelled)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync() ?? string.Empty;

                var sortedQuery = ProductVariantHelper.ApplySorting(
                    filteredQuery,
                    normalized.SortColumn,
                    normalized.SortMode);

                var pageRows = await sortedQuery
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize)
                    .Select(pv => new
                    {
                        pv.IdProductVariant,
                        pv.FkProduct,
                        pv.Sku,
                        pv.VariantLabel,
                        pv.Mrp,
                        pv.SellingPrice,
                        pv.IsActive,
                        pv.IsDefault,
                        pv.Cancelled,
                        pv.CreatedAt
                    })
                    .ToListAsync();

                var ids = pageRows.Select(r => r.IdProductVariant).ToList();
                var comboByVariant = await BuildCombinationMapsAsync(ids);

                var rows = pageRows.Select(pv =>
                {
                    comboByVariant.Labels.TryGetValue(pv.IdProductVariant, out var shortLabel);
                    comboByVariant.Combos.TryGetValue(pv.IdProductVariant, out var longCombo);
                    comboByVariant.Signatures.TryGetValue(pv.IdProductVariant, out var sig);

                    return new ProductVariant
                    {
                        ID_ProductVariant = pv.IdProductVariant,
                        FK_Product = pv.FkProduct,
                        SKU = pv.Sku,
                        VariantLabel = pv.VariantLabel,
                        Combination = longCombo ?? string.Empty,
                        AttributeSignature = string.IsNullOrEmpty(shortLabel) ? (sig ?? string.Empty) : shortLabel!,
                        MRP = pv.Mrp,
                        SellingPrice = pv.SellingPrice,
                        Price = pv.SellingPrice,
                        ProductName = productName,
                        IsActive = pv.IsActive,
                        IsDefault = pv.IsDefault,
                        Cancelled = pv.Cancelled,
                        CreatedAt = pv.CreatedAt
                    };
                }).ToList();

                return new TableOutput<ProductVariant>
                {
                    TableData = rows,
                    TableSettings = new TableOutput_Settings
                    {
                        PageIndex = normalized.PageIndex,
                        PageSize = normalized.PageSize,
                        TotalCount = totalCount
                    }
                };
            }
            catch
            {
                return ProductVariantHelper.EmptyTableOutput(input);
            }
        }

        public async Task<ProductVariantDetail?> GetProductVariantByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            try
            {
                var row = await _dbContext.ProductVariants.AsNoTracking()
                    .FirstOrDefaultAsync(pv => pv.IdProductVariant == id && !pv.Cancelled);

                if (row == null)
                {
                    return null;
                }

                var attributes = await (
                    from pva in _dbContext.ProductVariantAttributes.AsNoTracking()
                    join v in _dbContext.Variants.AsNoTracking() on pva.FkVariant equals v.IdVariant
                    join vv in _dbContext.VariantValues.AsNoTracking() on pva.FkVariantValue equals vv.IdVariantValue
                    where pva.FkProductVariant == id
                    orderby v.Name
                    select new VariantAttributeDetail
                    {
                        FK_Variant = pva.FkVariant,
                        VariantName = v.Name,
                        FK_VariantValue = pva.FkVariantValue,
                        ValueName = vv.Name
                    }).ToListAsync();

                return new ProductVariantDetail
                {
                    ID_ProductVariant = row.IdProductVariant,
                    FK_Product = row.FkProduct,
                    SKU = row.Sku,
                    VariantLabel = row.VariantLabel,
                    MRP = row.Mrp,
                    SellingPrice = row.SellingPrice,
                    IsActive = row.IsActive,
                    IsDefault = row.IsDefault,
                    CreatedAt = row.CreatedAt,
                    Attributes = attributes,
                    VariantValues = attributes
                        .Select(a => new VariantValueRowDetail
                        {
                            VariantId = a.FK_Variant,
                            VariantValueId = a.FK_VariantValue
                        })
                        .ToList()
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<CommonResponse> CreateProductVariantAsync(ProductVariantUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                var normalized = ProductVariantHelper.NormalizeUpdateInput(input);

                if (normalized.FK_Product <= 0)
                {
                    return Fail("Invalid product.");
                }

                if (normalized.SKU.Length == 0)
                {
                    return Fail("Please enter SKU.");
                }

                if (normalized.SellingPrice <= 0)
                {
                    return Fail("Selling price must be greater than zero.");
                }

                if (normalized.VariantValues.Count == 0)
                {
                    return Fail("At least one variant value is required.");
                }

                var formErr = ProductVariantHelper.ValidateVariantValueForm(normalized.VariantValues);
                if (formErr != null)
                {
                    return Fail(formErr);
                }

                if (!await ProductExistsAsync(normalized.FK_Product))
                {
                    return Fail("Invalid product.");
                }

                var valueIds = normalized.VariantValues.Select(v => v.VariantValueId).ToList();

                var resolved = await ResolveVariantPairsAsync(normalized.VariantValues);
                if (resolved == null)
                {
                    return Fail("One or more variant values do not match the selected variant, or are inactive.");
                }

                var comboErr = ProductVariantHelper.ValidateCombination(resolved);
                if (comboErr != null)
                {
                    return Fail(comboErr);
                }

                if (await SkuExistsAsync(normalized.SKU, excludeVariantId: 0))
                {
                    return Fail($"SKU \"{normalized.SKU}\" already exists.");
                }

                if (await CombinationExistsAsync(normalized.FK_Product, valueIds, excludeVariantId: 0))
                {
                    return Fail("This variant combination already exists for this product.");
                }

                var labelRows = resolved.Select(r => (r.VariantName, r.ValueName)).ToList();
                var autoLabel = ProductVariantHelper.GenerateVariantLabel(labelRows);
                var variantLabel = string.IsNullOrEmpty(normalized.VariantLabel)
                    ? autoLabel
                    : normalized.VariantLabel;

                if (variantLabel.Length == 0)
                {
                    return Fail("Unable to build variant label; please enter a label.");
                }

                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    if (normalized.IsDefault)
                    {
                        await ClearDefaultFlagsForProductAsync(normalized.FK_Product, exceptVariantId: 0);
                    }

                    var entity = new ProductVariantEntity
                    {
                        FkProduct = normalized.FK_Product,
                        Sku = normalized.SKU,
                        VariantLabel = variantLabel,
                        Mrp = normalized.MRP,
                        SellingPrice = normalized.SellingPrice,
                        IsActive = normalized.IsActive,
                        IsDefault = normalized.IsDefault,
                        CreatedAt = DateTime.Now,
                        Cancelled = false
                    };

                    _dbContext.ProductVariants.Add(entity);
                    await _dbContext.SaveChangesAsync();

                    foreach (var r in resolved)
                    {
                        _dbContext.ProductVariantAttributes.Add(new ProductVariantAttributeEntity
                        {
                            FkProductVariant = entity.IdProductVariant,
                            FkVariant = r.FkVariant,
                            FkVariantValue = r.FkVariantValue
                        });
                    }

                    await _dbContext.SaveChangesAsync();
                    await tx.CommitAsync();

                    return Ok(entity.IdProductVariant, "Product variant created successfully.");
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while creating product variant: {ex.Message}");
            }
        }

        public async Task<CommonResponse> UpdateProductVariantAsync(ProductVariantUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                var normalized = ProductVariantHelper.NormalizeUpdateInput(input);

                if (normalized.ID_ProductVariant <= 0)
                {
                    return Fail("Invalid product variant ID.");
                }

                if (normalized.FK_Product <= 0)
                {
                    return Fail("Invalid product.");
                }

                if (normalized.SKU.Length == 0)
                {
                    return Fail("Please enter SKU.");
                }

                if (normalized.SellingPrice <= 0)
                {
                    return Fail("Selling price must be greater than zero.");
                }

                if (normalized.VariantValues.Count == 0)
                {
                    return Fail("At least one variant value is required.");
                }

                var formErrUpdate = ProductVariantHelper.ValidateVariantValueForm(normalized.VariantValues);
                if (formErrUpdate != null)
                {
                    return Fail(formErrUpdate);
                }

                if (!await ProductExistsAsync(normalized.FK_Product))
                {
                    return Fail("Invalid product.");
                }

                var valueIdsUpdate = normalized.VariantValues.Select(v => v.VariantValueId).ToList();

                var resolved = await ResolveVariantPairsAsync(normalized.VariantValues);
                if (resolved == null)
                {
                    return Fail("One or more variant values do not match the selected variant, or are inactive.");
                }

                var comboErr = ProductVariantHelper.ValidateCombination(resolved);
                if (comboErr != null)
                {
                    return Fail(comboErr);
                }

                if (await SkuExistsAsync(normalized.SKU, excludeVariantId: normalized.ID_ProductVariant))
                {
                    return Fail($"SKU \"{normalized.SKU}\" already exists.");
                }

                if (await CombinationExistsAsync(normalized.FK_Product, valueIdsUpdate, excludeVariantId: normalized.ID_ProductVariant))
                {
                    return Fail("This variant combination already exists for this product.");
                }

                var entity = await _dbContext.ProductVariants
                    .FirstOrDefaultAsync(pv => pv.IdProductVariant == normalized.ID_ProductVariant);

                if (entity == null)
                {
                    return Fail("Invalid product variant ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This product variant is deleted and cannot be edited.");
                }

                if (entity.FkProduct != normalized.FK_Product)
                {
                    return Fail("Product cannot be changed for an existing SKU.");
                }

                var labelRows = resolved.Select(r => (r.VariantName, r.ValueName)).ToList();
                var autoLabel = ProductVariantHelper.GenerateVariantLabel(labelRows);
                var variantLabel = string.IsNullOrEmpty(normalized.VariantLabel)
                    ? autoLabel
                    : normalized.VariantLabel;

                if (variantLabel.Length == 0)
                {
                    return Fail("Unable to build variant label; please enter a label.");
                }

                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    if (normalized.IsDefault)
                    {
                        await ClearDefaultFlagsForProductAsync(normalized.FK_Product, exceptVariantId: normalized.ID_ProductVariant);
                    }

                    entity.Sku = normalized.SKU;
                    entity.VariantLabel = variantLabel;
                    entity.Mrp = normalized.MRP;
                    entity.SellingPrice = normalized.SellingPrice;
                    entity.IsActive = normalized.IsActive;
                    entity.IsDefault = normalized.IsDefault;

                    var existingAttrs = await _dbContext.ProductVariantAttributes
                        .Where(a => a.FkProductVariant == entity.IdProductVariant)
                        .ToListAsync();

                    _dbContext.ProductVariantAttributes.RemoveRange(existingAttrs);

                    foreach (var r in resolved)
                    {
                        _dbContext.ProductVariantAttributes.Add(new ProductVariantAttributeEntity
                        {
                            FkProductVariant = entity.IdProductVariant,
                            FkVariant = r.FkVariant,
                            FkVariantValue = r.FkVariantValue
                        });
                    }

                    await _dbContext.SaveChangesAsync();
                    await tx.CommitAsync();

                    return Ok(normalized.ID_ProductVariant, "Product variant updated successfully.");
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while updating product variant: {ex.Message}");
            }
        }

        public async Task<CommonResponse> DeleteProductVariantAsync(ProductVariantDeleteInput input)
        {
            if (input == null)
            {
                return Fail("Invalid product variant ID.");
            }

            try
            {
                if (input.ID_ProductVariant <= 0)
                {
                    return Fail("Invalid product variant ID.");
                }

                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var entity = await _dbContext.ProductVariants
                        .FirstOrDefaultAsync(pv => pv.IdProductVariant == input.ID_ProductVariant);

                    if (entity == null)
                    {
                        await tx.RollbackAsync();
                        return Fail("Invalid product variant ID.");
                    }

                    if (entity.Cancelled)
                    {
                        await tx.RollbackAsync();
                        return Fail("This product variant is already deleted.");
                    }

                    var attrs = await _dbContext.ProductVariantAttributes
                        .Where(a => a.FkProductVariant == entity.IdProductVariant)
                        .ToListAsync();

                    _dbContext.ProductVariantAttributes.RemoveRange(attrs);

                    entity.Cancelled = true;
                    entity.CancelledOn = DateTime.Now;

                    await _dbContext.SaveChangesAsync();
                    await tx.CommitAsync();

                    return Ok(input.ID_ProductVariant, "Product variant deleted successfully.");
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while deleting product variant: {ex.Message}");
            }
        }

        private async Task<(Dictionary<int, string> Labels, Dictionary<int, string> Combos, Dictionary<int, string> Signatures)>
            BuildCombinationMapsAsync(IReadOnlyList<int> productVariantIds)
        {
            var labels = new Dictionary<int, string>();
            var combos = new Dictionary<int, string>();
            var signatures = new Dictionary<int, string>();

            if (productVariantIds.Count == 0)
            {
                return (labels, combos, signatures);
            }

            var rows = await (
                from pva in _dbContext.ProductVariantAttributes.AsNoTracking()
                join vv in _dbContext.VariantValues.AsNoTracking() on pva.FkVariantValue equals vv.IdVariantValue
                join v in _dbContext.Variants.AsNoTracking() on pva.FkVariant equals v.IdVariant
                where productVariantIds.Contains(pva.FkProductVariant)
                select new
                {
                    pva.FkProductVariant,
                    VariantName = v.Name,
                    ValueName = vv.Name,
                    pva.FkVariantValue
                }).ToListAsync();

            foreach (var g in rows.GroupBy(r => r.FkProductVariant))
            {
                var tupleRows = g.Select(x => (x.VariantName, x.ValueName)).ToList();
                labels[g.Key] = ProductVariantHelper.GenerateVariantLabel(tupleRows);
                combos[g.Key] = ProductVariantHelper.GenerateCombination(tupleRows);
                var sig = string.Join("-", g.Select(x => x.FkVariantValue).OrderBy(x => x));
                signatures[g.Key] = sig;
            }

            return (labels, combos, signatures);
        }

        private async Task<List<(int FkVariant, int FkVariantValue, string VariantName, string ValueName)>?> ResolveVariantPairsAsync(
            IReadOnlyList<ProductVariantValueRowInput> pairs)
        {
            var distinctIds = pairs.Select(p => p.VariantValueId).Distinct().ToList();
            if (distinctIds.Count == 0)
            {
                return new List<(int, int, string, string)>();
            }

            var rows = await (
                from vv in _dbContext.VariantValues.AsNoTracking()
                join v in _dbContext.Variants.AsNoTracking() on vv.FkVariant equals v.IdVariant
                where distinctIds.Contains(vv.IdVariantValue) &&
                      !vv.Cancelled &&
                      !v.Cancelled &&
                      v.IsActive
                select new
                {
                    vv.IdVariantValue,
                    vv.FkVariant,
                    VariantName = v.Name,
                    ValueName = vv.Name
                }).ToListAsync();

            if (rows.Count != distinctIds.Count)
            {
                return null;
            }

            var byValueId = rows.ToDictionary(r => r.IdVariantValue, r => r);
            var ordered = new List<(int FkVariant, int FkVariantValue, string VariantName, string ValueName)>();
            foreach (var p in pairs)
            {
                if (!byValueId.TryGetValue(p.VariantValueId, out var hit))
                {
                    return null;
                }

                if (hit.FkVariant != p.VariantId)
                {
                    return null;
                }

                ordered.Add((hit.FkVariant, hit.IdVariantValue, hit.VariantName, hit.ValueName));
            }

            return ordered;
        }

        private async Task<bool> CombinationExistsAsync(int fkProduct, IReadOnlyList<int> valueIds, int excludeVariantId)
        {
            var candidates = await _dbContext.ProductVariants.AsNoTracking()
                .Where(pv => pv.FkProduct == fkProduct && !pv.Cancelled &&
                             (excludeVariantId == 0 || pv.IdProductVariant != excludeVariantId))
                .Select(pv => pv.IdProductVariant)
                .ToListAsync();

            if (candidates.Count == 0)
            {
                return false;
            }

            var attrs = await _dbContext.ProductVariantAttributes.AsNoTracking()
                .Where(a => candidates.Contains(a.FkProductVariant))
                .Select(a => new { a.FkProductVariant, a.FkVariantValue })
                .ToListAsync();

            foreach (var grp in attrs.GroupBy(a => a.FkProductVariant))
            {
                var theirs = grp.Select(x => x.FkVariantValue).ToList();
                if (ProductVariantHelper.SameVariantValueSet(theirs, valueIds))
                {
                    return true;
                }
            }

            return false;
        }

        private Task<bool> SkuExistsAsync(string sku, int excludeVariantId) =>
            _dbContext.ProductVariants.AnyAsync(pv =>
                pv.Sku == sku &&
                (excludeVariantId == 0 || pv.IdProductVariant != excludeVariantId));

        private Task<bool> ProductExistsAsync(int productId) =>
            _dbContext.Products.AnyAsync(p => p.IdProduct == productId && !p.Cancelled);

        private async Task ClearDefaultFlagsForProductAsync(int fkProduct, int exceptVariantId)
        {
            var others = await _dbContext.ProductVariants
                .Where(pv => pv.FkProduct == fkProduct && !pv.Cancelled &&
                             (exceptVariantId == 0 || pv.IdProductVariant != exceptVariantId) &&
                             pv.IsDefault)
                .ToListAsync();

            foreach (var o in others)
            {
                o.IsDefault = false;
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
