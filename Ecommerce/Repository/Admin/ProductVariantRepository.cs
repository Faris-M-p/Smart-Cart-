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
                            pva.FK_ProductVariant == pv.ID_ProductVariant &&
                            fids.Contains(pva.FK_Variant)));
                }

                if (normalized.FilterVariantValueIds.Count > 0)
                {
                    var vids = normalized.FilterVariantValueIds;
                    filteredQuery = filteredQuery.Where(pv =>
                        _dbContext.ProductVariantAttributes.AsNoTracking().Any(pva =>
                            pva.FK_ProductVariant == pv.ID_ProductVariant &&
                            vids.Contains(pva.FK_VariantValue)));
                }

                var totalCount = await filteredQuery.LongCountAsync();

                var productName = await _dbContext.Products.AsNoTracking()
                    .Where(p => p.ID_Product == input.FK_Product && !p.Cancelled)
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
                        pv.ID_ProductVariant,
                        pv.FK_Product,
                        pv.SKU,
                        pv.VariantLabel,
                        pv.MRP,
                        pv.SellingPrice,
                        pv.IsActive,
                        pv.SellOnline,
                        pv.IsDefault,
                        pv.Cancelled,
                        pv.CreatedAt,
                        TotalImages = _dbContext.ProductVariantImages.Count(i => i.FK_ProductVariant == pv.ID_ProductVariant),
                        ImageUrl = (
                            from i in _dbContext.ProductVariantImages
                            where i.FK_ProductVariant == pv.ID_ProductVariant
                            orderby i.IsPrimary descending, i.DisplayOrder, i.ID_ProductVariantImage
                            select i.ImageUrl
                        ).FirstOrDefault()
                    })
                    .ToListAsync();

                var ids = pageRows.Select(r => r.ID_ProductVariant).ToList();
                var comboByVariant = await BuildCombinationMapsAsync(ids);

                var rows = pageRows.Select(pv =>
                {
                    comboByVariant.Labels.TryGetValue(pv.ID_ProductVariant, out var shortLabel);
                    comboByVariant.Combos.TryGetValue(pv.ID_ProductVariant, out var longCombo);
                    comboByVariant.Signatures.TryGetValue(pv.ID_ProductVariant, out var sig);

                    return new ProductVariant
                    {
                        ID_ProductVariant = pv.ID_ProductVariant,
                        FK_Product = pv.FK_Product,
                        SKU = pv.SKU,
                        VariantLabel = pv.VariantLabel,
                        Combination = longCombo ?? string.Empty,
                        AttributeSignature = string.IsNullOrEmpty(shortLabel) ? (sig ?? string.Empty) : shortLabel!,
                        MRP = pv.MRP,
                        SellingPrice = pv.SellingPrice,
                        Price = pv.SellingPrice,
                        ProductName = productName,
                        IsActive = pv.IsActive,
                        SellOnline = pv.SellOnline,
                        IsDefault = pv.IsDefault,
                        Cancelled = pv.Cancelled,
                        CreatedAt = pv.CreatedAt,
                        TotalImages = pv.TotalImages,
                        ImageUrl = pv.ImageUrl
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
                throw;
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
                    .FirstOrDefaultAsync(pv => pv.ID_ProductVariant == id && !pv.Cancelled);

                if (row == null)
                {
                    return null;
                }

                var attributes = await (
                    from pva in _dbContext.ProductVariantAttributes.AsNoTracking()
                    join v in _dbContext.Variants.AsNoTracking() on pva.FK_Variant equals v.ID_Variant
                    join vv in _dbContext.VariantValues.AsNoTracking() on pva.FK_VariantValue equals vv.ID_VariantValue
                    where pva.FK_ProductVariant == id
                    orderby v.Name
                    select new VariantAttributeDetail
                    {
                        FK_Variant = pva.FK_Variant,
                        VariantName = v.Name,
                        FK_VariantValue = pva.FK_VariantValue,
                        ValueName = vv.Name
                    }).ToListAsync();

                var images = await _dbContext.ProductVariantImages.AsNoTracking()
                    .Where(i => i.FK_ProductVariant == id)
                    .OrderBy(i => i.DisplayOrder)
                    .ThenBy(i => i.ID_ProductVariantImage)
                    .Select(i => new ProductVariantImageDto
                    {
                        ID_ProductVariantImage = i.ID_ProductVariantImage,
                        FK_ProductVariant = i.FK_ProductVariant,
                        ImageUrl = i.ImageUrl,
                        IsPrimary = i.IsPrimary,
                        DisplayOrder = i.DisplayOrder
                    })
                    .ToListAsync();

                return new ProductVariantDetail
                {
                    ID_ProductVariant = row.ID_ProductVariant,
                    FK_Product = row.FK_Product,
                    SKU = row.SKU,
                    VariantLabel = row.VariantLabel,
                    MRP = row.MRP,
                    SellingPrice = row.SellingPrice,
                    IsActive = row.IsActive,
                    SellOnline = row.SellOnline,
                    IsDefault = row.IsDefault,
                    CreatedAt = row.CreatedAt,
                    Attributes = attributes,
                    VariantValues = attributes
                        .Select(a => new VariantValueRowDetail
                        {
                            VariantId = a.FK_Variant,
                            VariantValueId = a.FK_VariantValue
                        })
                        .ToList(),
                    Images = images,
                    PrimaryImageId = images.FirstOrDefault(x => x.IsPrimary)?.ID_ProductVariantImage,
                    ImageOrder = images.Select(x => x.ID_ProductVariantImage).ToList()
                };
            }
            catch
            {
                throw;
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

                if (normalized.IsDefault)
                {
                    await ClearDefaultFlagsForProductAsync(normalized.FK_Product, exceptVariantId: 0);
                }

                var entity = new ProductVariantEntity
                {
                    FK_Product = normalized.FK_Product,
                    SKU = normalized.SKU,
                    Barcode = normalized.SKU,
                    VariantLabel = variantLabel,
                    MRP = normalized.MRP,
                    SellingPrice = normalized.SellingPrice,
                    IsActive = normalized.IsActive,
                    SellOnline = normalized.SellOnline,
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
                        FK_ProductVariant = entity.ID_ProductVariant,
                        FK_Variant = r.FkVariant,
                        FK_VariantValue = r.FkVariantValue
                    });
                }

                await _dbContext.SaveChangesAsync();
                return Ok(entity.ID_ProductVariant, "Product variant created successfully.");
            }
            catch
            {
                throw;
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
                    .FirstOrDefaultAsync(pv => pv.ID_ProductVariant == normalized.ID_ProductVariant);

                if (entity == null)
                {
                    return Fail("Invalid product variant ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This product variant is deleted and cannot be edited.");
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

                if (normalized.IsDefault)
                {
                    await ClearDefaultFlagsForProductAsync(normalized.FK_Product, exceptVariantId: normalized.ID_ProductVariant);
                }

                entity.SKU = normalized.SKU;
                entity.Barcode = normalized.SKU;
                entity.FK_Product = normalized.FK_Product;
                entity.VariantLabel = variantLabel;
                entity.MRP = normalized.MRP;
                entity.SellingPrice = normalized.SellingPrice;
                entity.IsActive = normalized.IsActive;
                entity.SellOnline = normalized.SellOnline;
                entity.IsDefault = normalized.IsDefault;

                var existingAttrs = await _dbContext.ProductVariantAttributes
                    .Where(a => a.FK_ProductVariant == entity.ID_ProductVariant)
                    .ToListAsync();

                _dbContext.ProductVariantAttributes.RemoveRange(existingAttrs);

                foreach (var r in resolved)
                {
                    _dbContext.ProductVariantAttributes.Add(new ProductVariantAttributeEntity
                    {
                        FK_ProductVariant = entity.ID_ProductVariant,
                        FK_Variant = r.FkVariant,
                        FK_VariantValue = r.FkVariantValue
                    });
                }

                await _dbContext.SaveChangesAsync();
                return Ok(normalized.ID_ProductVariant, "Product variant updated successfully.");
            }
            catch
            {
                throw;
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
                        .FirstOrDefaultAsync(pv => pv.ID_ProductVariant == input.ID_ProductVariant);

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

                    var activeSkuCount = await _dbContext.ProductVariants
                        .CountAsync(pv => pv.FK_Product == entity.FK_Product && !pv.Cancelled);
                    if (activeSkuCount <= 1)
                    {
                        await tx.RollbackAsync();
                        return Fail("Each product must have at least one SKU.");
                    }

                    var attrs = await _dbContext.ProductVariantAttributes
                        .Where(a => a.FK_ProductVariant == entity.ID_ProductVariant)
                        .ToListAsync();

                    _dbContext.ProductVariantAttributes.RemoveRange(attrs);

                    var images = await _dbContext.ProductVariantImages
                        .Where(a => a.FK_ProductVariant == entity.ID_ProductVariant)
                        .ToListAsync();
                    _dbContext.ProductVariantImages.RemoveRange(images);

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
            catch
            {
                throw;
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
                join vv in _dbContext.VariantValues.AsNoTracking() on pva.FK_VariantValue equals vv.ID_VariantValue
                join v in _dbContext.Variants.AsNoTracking() on pva.FK_Variant equals v.ID_Variant
                where productVariantIds.Contains(pva.FK_ProductVariant)
                select new
                {
                    pva.FK_ProductVariant,
                    VariantName = v.Name,
                    ValueName = vv.Name,
                    pva.FK_VariantValue
                }).ToListAsync();

            foreach (var g in rows.GroupBy(r => r.FK_ProductVariant))
            {
                var tupleRows = g.Select(x => (x.VariantName, x.ValueName)).ToList();
                labels[g.Key] = ProductVariantHelper.GenerateVariantLabel(tupleRows);
                combos[g.Key] = ProductVariantHelper.GenerateCombination(tupleRows);
                var sig = string.Join("-", g.Select(x => x.FK_VariantValue).OrderBy(x => x));
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
                join v in _dbContext.Variants.AsNoTracking() on vv.FK_Variant equals v.ID_Variant
                where distinctIds.Contains(vv.ID_VariantValue) &&
                      !vv.Cancelled &&
                      !v.Cancelled &&
                      v.IsActive
                select new
                {
                    vv.ID_VariantValue,
                    vv.FK_Variant,
                    VariantName = v.Name,
                    ValueName = vv.Name
                }).ToListAsync();

            if (rows.Count != distinctIds.Count)
            {
                return null;
            }

            var byValueId = rows.ToDictionary(r => r.ID_VariantValue, r => r);
            var ordered = new List<(int FkVariant, int FkVariantValue, string VariantName, string ValueName)>();
            foreach (var p in pairs)
            {
                if (!byValueId.TryGetValue(p.VariantValueId, out var hit))
                {
                    return null;
                }

                if (hit.FK_Variant != p.VariantId)
                {
                    return null;
                }

                ordered.Add((hit.FK_Variant, hit.ID_VariantValue, hit.VariantName, hit.ValueName));
            }

            return ordered;
        }

        private async Task<bool> CombinationExistsAsync(int fkProduct, IReadOnlyList<int> valueIds, int excludeVariantId)
        {
            var candidates = await _dbContext.ProductVariants.AsNoTracking()
                .Where(pv => pv.FK_Product == fkProduct && !pv.Cancelled && pv.IsActive &&
                             (excludeVariantId == 0 || pv.ID_ProductVariant != excludeVariantId))
                .Select(pv => pv.ID_ProductVariant)
                .ToListAsync();

            if (candidates.Count == 0)
            {
                return false;
            }

            var attrs = await _dbContext.ProductVariantAttributes.AsNoTracking()
                .Where(a => candidates.Contains(a.FK_ProductVariant))
                .Select(a => new { a.FK_ProductVariant, a.FK_VariantValue })
                .ToListAsync();

            foreach (var grp in attrs.GroupBy(a => a.FK_ProductVariant))
            {
                var theirs = grp.Select(x => x.FK_VariantValue).ToList();
                if (ProductVariantHelper.SameVariantValueSet(theirs, valueIds))
                {
                    return true;
                }
            }

            return false;
        }

        private Task<bool> SkuExistsAsync(string sku, int excludeVariantId) =>
            _dbContext.ProductVariants.AnyAsync(pv =>
                pv.SKU == sku &&
                (excludeVariantId == 0 || pv.ID_ProductVariant != excludeVariantId));

        private Task<bool> ProductExistsAsync(int productId) =>
            _dbContext.Products.AnyAsync(p => p.ID_Product == productId && !p.Cancelled);

        private async Task ClearDefaultFlagsForProductAsync(int fkProduct, int exceptVariantId)
        {
            var others = await _dbContext.ProductVariants
                .Where(pv => pv.FK_Product == fkProduct && !pv.Cancelled &&
                             (exceptVariantId == 0 || pv.ID_ProductVariant != exceptVariantId) &&
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
