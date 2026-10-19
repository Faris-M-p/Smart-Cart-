using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly SmartCartDbContext _db;

    public ProductRepository(SmartCartDbContext db) => _db = db;

    // ═══════════════════════════════════════════════════════════════
    // PRODUCT
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputProductSelect?> GetProductAsync(InputProductSelect input, CancellationToken ct = default)
        => await _db.Products
            .Where(x => x.IdProduct == input.IdProduct && !x.Cancelled)
            .Select(ProductProjection())
            .FirstOrDefaultAsync(ct);

    public async Task<OutputProductList> ListProductsAsync(InputProductList input, CancellationToken ct = default)
    {
        var q = _db.Products.Where(x => !x.Cancelled);

        if (input.ActiveOnly.HasValue)
        {
            var active = input.ActiveOnly.Value;
            q = q.Where(x => x.IsActive == active);
        }
        if (input.FkCategory is > 0)
        {
            var category = input.FkCategory.Value;
            q = q.Where(x => x.FkCategory == category);
        }
        if (input.FkSubcategory is > 0)
        {
            var subCategory = input.FkSubcategory.Value;
            q = q.Where(x => x.FkSubcategory == subCategory);
        }
        if (input.FkBrand is > 0)
        {
            var brand = input.FkBrand.Value;
            q = q.Where(x => x.FkBrand == brand);
        }

        var term = (input.Search ?? string.Empty).Trim().ToLowerInvariant();
        if (term.Length > 0)
        {
            q = q.Where(x =>
                x.ProductName.ToLower().Contains(term) ||
                x.ProductCode.ToLower().Contains(term) ||
                (x.Sku != null && x.Sku.ToLower().Contains(term)) ||
                (x.Barcode != null && x.Barcode.ToLower().Contains(term)) ||
                _db.ProductVariants.Any(v => v.FkProduct == x.IdProduct && !v.Cancelled &&
                    ((v.SkuCode != null && v.SkuCode.ToLower().Contains(term)) ||
                     (v.Barcode != null && v.Barcode.ToLower().Contains(term)))));
        }

        var sort = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();
        IOrderedQueryable<Product> ordered = sort switch
        {
            "productcode" => input.SortDesc ? q.OrderByDescending(x => x.ProductCode) : q.OrderBy(x => x.ProductCode),
            "category" => input.SortDesc ? q.OrderByDescending(x => x.Category!.CategoryName) : q.OrderBy(x => x.Category!.CategoryName),
            "sellingprice" => input.SortDesc ? q.OrderByDescending(x => x.SellingPrice) : q.OrderBy(x => x.SellingPrice),
            "isactive" => input.SortDesc ? q.OrderByDescending(x => x.IsActive) : q.OrderBy(x => x.IsActive),
            _ => input.SortDesc ? q.OrderByDescending(x => x.ProductName) : q.OrderBy(x => x.ProductName)
        };
        var stable = ordered.ThenBy(x => x.IdProduct);

        var total = await q.CountAsync(ct);
        var pageSize = Math.Max(1, input.PageSize);
        var lastPage = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        var page = Math.Min(Math.Max(1, input.Page), lastPage);

        var items = await stable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ProductProjection())
            .ToListAsync(ct);

        return new OutputProductList
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Items = items,
            Filter = input
        };
    }

    public async Task<OutputProductCheck> CheckProductAsync(InputProductCheck input, CancellationToken ct = default)
    {
        var result = new OutputProductCheck();

        if (input.IdProduct > 0)
            result.ProductExists = await _db.Products.AnyAsync(x => x.IdProduct == input.IdProduct && !x.Cancelled, ct);

        // Unique keys are checked against every row, cancelled ones included (the database constraints do the same).
        var code = (input.ProductCode ?? string.Empty).ToLower();
        result.CodeInUse = await _db.Products
            .AnyAsync(x => x.IdProduct != input.IdProduct && x.ProductCode.ToLower() == code, ct);

        if (!string.IsNullOrEmpty(input.Sku))
        {
            var sku = input.Sku.ToLower();
            result.SkuInUse =
                await _db.Products.AnyAsync(x => x.IdProduct != input.IdProduct && x.Sku != null && x.Sku.ToLower() == sku, ct) ||
                await _db.ProductVariants.AnyAsync(v => v.SkuCode != null && v.SkuCode.ToLower() == sku, ct);
        }

        if (!string.IsNullOrEmpty(input.Barcode))
        {
            var barcode = input.Barcode.ToLower();
            result.BarcodeInUse =
                await _db.Products.AnyAsync(x => x.IdProduct != input.IdProduct && x.Barcode != null && x.Barcode.ToLower() == barcode, ct) ||
                await _db.ProductVariants.AnyAsync(v => v.Barcode != null && v.Barcode.ToLower() == barcode, ct);
        }

        if (input.FkCategory.HasValue)
        {
            var category = input.FkCategory.Value;
            result.CategoryMissing = !await _db.Categories.AnyAsync(x => x.IdCategory == category && !x.Cancelled, ct);
        }

        if (input.FkSubcategory.HasValue)
        {
            var subCategoryId = input.FkSubcategory.Value;
            var subCategory = await _db.SubCategories
                .Where(x => x.IdSubcategory == subCategoryId && !x.Cancelled)
                .Select(x => new { x.FkCategory })
                .FirstOrDefaultAsync(ct);

            if (subCategory is null)
                result.SubCategoryMissing = true;
            else if (!input.FkCategory.HasValue || subCategory.FkCategory != input.FkCategory.Value)
                result.SubCategoryOutsideCategory = true;
        }

        if (input.FkBrand.HasValue)
        {
            var brand = input.FkBrand.Value;
            result.BrandMissing = !await _db.Brands.AnyAsync(x => x.IdBrand == brand && !x.Cancelled, ct);
        }

        if (input.FkUnit.HasValue)
        {
            var unit = input.FkUnit.Value;
            result.UnitMissing = !await _db.Units.AnyAsync(x => x.IdUnit == unit && !x.Cancelled, ct);
        }

        if (input.FkTax.HasValue)
        {
            var tax = input.FkTax.Value;
            result.TaxMissing = !await _db.Taxes.AnyAsync(x => x.IdTax == tax && !x.Cancelled, ct);
        }

        return result;
    }

    public async Task<OutputProductCreate> CreateProductAsync(InputProductCreate input, CancellationToken ct = default)
    {
        var entity = new Product
        {
            ProductCode = input.ProductCode,
            ProductName = input.ProductName,
            Description = input.Description,
            Sku = input.Sku,
            Barcode = input.Barcode,
            FkCategory = input.FkCategory,
            FkSubcategory = input.FkSubcategory,
            FkBrand = input.FkBrand,
            FkUnit = input.FkUnit,
            FkTax = input.FkTax,
            CostPrice = input.CostPrice,
            SellingPrice = input.SellingPrice,
            Mrp = input.Mrp,
            ReorderLevel = input.ReorderLevel,
            IsActive = input.IsActive,
            IsOnline = input.IsOnline,
            CreatedAt = DateTime.UtcNow
        };
        _db.Products.Add(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsConstraintViolation(ex))
        {
            _db.Entry(entity).State = EntityState.Detached;
            return new OutputProductCreate { ResponseCode = 2, ResponseMsg = ConstraintMessage(ex), StatusCode = 409 };
        }

        return new OutputProductCreate
        {
            IdProduct = entity.IdProduct,
            ProductCode = entity.ProductCode,
            ProductName = entity.ProductName
        };
    }

    public async Task<OutputProductUpdate> UpdateProductAsync(InputProductUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(x => x.IdProduct == input.IdProduct && !x.Cancelled, ct);
        if (entity is null)
            return new OutputProductUpdate { ResponseCode = 1, ResponseMsg = "Product not found.", StatusCode = 404 };

        entity.ProductCode = input.ProductCode;
        entity.ProductName = input.ProductName;
        entity.Description = input.Description;
        entity.Sku = input.Sku;
        entity.Barcode = input.Barcode;
        entity.FkCategory = input.FkCategory;
        entity.FkSubcategory = input.FkSubcategory;
        entity.FkBrand = input.FkBrand;
        entity.FkUnit = input.FkUnit;
        entity.FkTax = input.FkTax;
        entity.CostPrice = input.CostPrice;
        entity.SellingPrice = input.SellingPrice;
        entity.Mrp = input.Mrp;
        entity.ReorderLevel = input.ReorderLevel;
        entity.IsActive = input.IsActive;
        entity.IsOnline = input.IsOnline;
        entity.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsConstraintViolation(ex))
        {
            return new OutputProductUpdate { ResponseCode = 2, ResponseMsg = ConstraintMessage(ex), StatusCode = 409 };
        }

        return new OutputProductUpdate { IdProduct = entity.IdProduct };
    }

    public async Task<OutputProductSetActive> SetProductActiveAsync(InputProductSetActive input, CancellationToken ct = default)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(x => x.IdProduct == input.IdProduct && !x.Cancelled, ct);
        if (entity is null)
            return new OutputProductSetActive { ResponseCode = 1, ResponseMsg = "Product not found.", StatusCode = 404 };

        if (entity.IsActive != input.IsActive)
        {
            entity.IsActive = input.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return new OutputProductSetActive { IdProduct = entity.IdProduct, IsActive = entity.IsActive };
    }

    // ═══════════════════════════════════════════════════════════════
    // DROPDOWN DATA / STOCK SUMMARY
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputProductLookup> GetProductLookupAsync(InputProductLookup input, CancellationToken ct = default)
    {
        var all = input.IncludeInactive;
        var categoryId = input.FkCategory ?? 0;
        var subCategoryId = input.FkSubcategory ?? 0;
        var brandId = input.FkBrand ?? 0;
        var unitId = input.FkUnit ?? 0;
        var taxId = input.FkTax ?? 0;

        var result = new OutputProductLookup();

        result.Categories = await _db.Categories
            .Where(x => !x.Cancelled && (all || x.IsActive || x.IdCategory == categoryId))
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.CategoryName)
            .Select(x => new OutputProductLookupItem
            {
                Id = x.IdCategory,
                Name = x.CategoryName,
                Code = x.CategoryCode,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);

        if (categoryId > 0)
        {
            result.SubCategories = await _db.SubCategories
                .Where(x => !x.Cancelled && x.FkCategory == categoryId && (all || x.IsActive || x.IdSubcategory == subCategoryId))
                .OrderBy(x => x.DisplayOrder).ThenBy(x => x.SubCategoryName)
                .Select(x => new OutputProductLookupItem
                {
                    Id = x.IdSubcategory,
                    Name = x.SubCategoryName,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }

        result.Brands = await _db.Brands
            .Where(x => !x.Cancelled && (all || x.IsActive || x.IdBrand == brandId))
            .OrderBy(x => x.BrandName)
            .Select(x => new OutputProductLookupItem
            {
                Id = x.IdBrand,
                Name = x.BrandName,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);

        if (input.IncludeUnitsAndTaxes)
        {
            result.Units = await _db.Units
                .Where(x => !x.Cancelled && (all || x.IsActive || x.IdUnit == unitId))
                .OrderBy(x => x.UnitName)
                .Select(x => new OutputProductLookupItem
                {
                    Id = x.IdUnit,
                    Name = x.UnitName,
                    Code = x.UnitCode,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);

            result.Taxes = await _db.Taxes
                .Where(x => !x.Cancelled && (all || x.IsActive || x.IdTax == taxId))
                .OrderBy(x => x.TaxName)
                .Select(x => new OutputProductLookupItem
                {
                    Id = x.IdTax,
                    Name = x.TaxName,
                    Code = x.TaxCode,
                    Rate = x.TaxRate,
                    IsInclusive = x.IsInclusive,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }

        return result;
    }

    public async Task<IReadOnlyList<OutputProductStockItem>> ListProductStockAsync(InputProductStockList input, CancellationToken ct = default)
        => await _db.Inventories
            .Where(x => x.FkStore == input.FkStore && x.FkProduct == input.FkProduct)
            .Select(x => new OutputProductStockItem
            {
                FkProductVariant = x.FkProductVariant,
                QuantityOnHand = x.QuantityOnHand,
                QuantityReserved = x.QuantityReserved,
                ReorderLevel = x.ReorderLevel
            })
            .ToListAsync(ct);

    // ═══════════════════════════════════════════════════════════════
    // VARIANT
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputVariantSelect?> GetVariantAsync(InputVariantSelect input, CancellationToken ct = default)
        => await _db.ProductVariants
            .Where(x => x.IdProductVariant == input.IdProductVariant && !x.Cancelled && !x.Product!.Cancelled)
            .Select(VariantProjection)
            .FirstOrDefaultAsync(ct);

    public async Task<OutputVariantList> ListVariantsAsync(InputVariantList input, CancellationToken ct = default)
    {
        var q = _db.ProductVariants.Where(x => !x.Cancelled && !x.Product!.Cancelled);

        var scopedProductId = input.FkProduct ?? 0;
        if (scopedProductId > 0)
            q = q.Where(x => x.FkProduct == scopedProductId);

        if (input.ActiveOnly.HasValue)
        {
            var active = input.ActiveOnly.Value;
            q = q.Where(x => x.IsActive == active);
        }

        var term = (input.Search ?? string.Empty).Trim().ToLowerInvariant();
        if (term.Length > 0)
        {
            q = q.Where(x =>
                x.VariantName.ToLower().Contains(term) ||
                (x.SkuCode != null && x.SkuCode.ToLower().Contains(term)) ||
                (x.Barcode != null && x.Barcode.ToLower().Contains(term)) ||
                x.Product!.ProductName.ToLower().Contains(term) ||
                x.Product!.ProductCode.ToLower().Contains(term));
        }

        var sort = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();
        IOrderedQueryable<ProductVariant> ordered = sort switch
        {
            "skucode" => input.SortDesc ? q.OrderByDescending(x => x.SkuCode) : q.OrderBy(x => x.SkuCode),
            "additionalprice" => input.SortDesc ? q.OrderByDescending(x => x.AdditionalPrice) : q.OrderBy(x => x.AdditionalPrice),
            "isactive" => input.SortDesc ? q.OrderByDescending(x => x.IsActive) : q.OrderBy(x => x.IsActive),
            "product" => input.SortDesc
                ? q.OrderByDescending(x => x.Product!.ProductName).ThenByDescending(x => x.VariantName)
                : q.OrderBy(x => x.Product!.ProductName).ThenBy(x => x.VariantName),
            _ => input.SortDesc ? q.OrderByDescending(x => x.VariantName) : q.OrderBy(x => x.VariantName)
        };
        var stable = ordered.ThenBy(x => x.IdProductVariant);

        var total = await q.CountAsync(ct);
        var pageSize = Math.Max(1, input.PageSize);
        var lastPage = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        var page = Math.Min(Math.Max(1, input.Page), lastPage);

        var items = await stable
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(VariantProjection)
            .ToListAsync(ct);

        var result = new OutputVariantList
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            FkProduct = scopedProductId,
            Items = items,
            Filter = input
        };

        if (scopedProductId > 0)
        {
            var header = await _db.Products
                .Where(x => x.IdProduct == scopedProductId && !x.Cancelled)
                .Select(x => new { x.ProductCode, x.ProductName, x.IsActive })
                .FirstOrDefaultAsync(ct);

            if (header is not null)
            {
                result.ProductCode = header.ProductCode;
                result.ProductName = header.ProductName;
                result.ProductIsActive = header.IsActive;
            }
        }

        return result;
    }

    public async Task<OutputVariantCheck> CheckVariantAsync(InputVariantCheck input, CancellationToken ct = default)
    {
        var result = new OutputVariantCheck();
        var productId = input.FkProduct;

        if (input.IdProductVariant > 0)
        {
            var stored = await _db.ProductVariants
                .Where(x => x.IdProductVariant == input.IdProductVariant && !x.Cancelled)
                .Select(x => new { x.FkProduct })
                .FirstOrDefaultAsync(ct);

            if (stored is null)
            {
                result.VariantExists = false;
                return result;
            }

            productId = stored.FkProduct;
        }

        result.FkProduct = productId;
        result.ProductExists = await _db.Products.AnyAsync(x => x.IdProduct == productId && !x.Cancelled, ct);
        if (!result.ProductExists)
            return result;

        var name = (input.VariantName ?? string.Empty).ToLower();
        result.NameInUse = await _db.ProductVariants
            .AnyAsync(x => x.FkProduct == productId && x.IdProductVariant != input.IdProductVariant && x.VariantName.ToLower() == name, ct);

        if (!string.IsNullOrEmpty(input.SkuCode))
        {
            var sku = input.SkuCode.ToLower();
            result.SkuInUse =
                await _db.ProductVariants.AnyAsync(x => x.IdProductVariant != input.IdProductVariant && x.SkuCode != null && x.SkuCode.ToLower() == sku, ct) ||
                await _db.Products.AnyAsync(x => x.Sku != null && x.Sku.ToLower() == sku, ct);
        }

        if (!string.IsNullOrEmpty(input.Barcode))
        {
            var barcode = input.Barcode.ToLower();
            result.BarcodeInUse =
                await _db.ProductVariants.AnyAsync(x => x.IdProductVariant != input.IdProductVariant && x.Barcode != null && x.Barcode.ToLower() == barcode, ct) ||
                await _db.Products.AnyAsync(x => x.Barcode != null && x.Barcode.ToLower() == barcode, ct);
        }

        return result;
    }

    public async Task<OutputVariantCreate> CreateVariantAsync(InputVariantCreate input, CancellationToken ct = default)
    {
        var entity = new ProductVariant
        {
            FkProduct = input.FkProduct,
            VariantName = input.VariantName,
            SkuCode = input.SkuCode,
            Barcode = input.Barcode,
            AdditionalPrice = input.AdditionalPrice,
            IsActive = input.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _db.ProductVariants.Add(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsConstraintViolation(ex))
        {
            _db.Entry(entity).State = EntityState.Detached;
            return new OutputVariantCreate { ResponseCode = 2, ResponseMsg = ConstraintMessage(ex), StatusCode = 409 };
        }

        return new OutputVariantCreate { IdProductVariant = entity.IdProductVariant, FkProduct = entity.FkProduct };
    }

    public async Task<OutputVariantUpdate> UpdateVariantAsync(InputVariantUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.ProductVariants.FirstOrDefaultAsync(x => x.IdProductVariant == input.IdProductVariant && !x.Cancelled, ct);
        if (entity is null)
            return new OutputVariantUpdate { ResponseCode = 1, ResponseMsg = "Variant not found.", StatusCode = 404 };

        // The parent product never changes on update (input.FkProduct is intentionally ignored).
        entity.VariantName = input.VariantName;
        entity.SkuCode = input.SkuCode;
        entity.Barcode = input.Barcode;
        entity.AdditionalPrice = input.AdditionalPrice;
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsConstraintViolation(ex))
        {
            return new OutputVariantUpdate { ResponseCode = 2, ResponseMsg = ConstraintMessage(ex), StatusCode = 409 };
        }

        return new OutputVariantUpdate { IdProductVariant = entity.IdProductVariant, FkProduct = entity.FkProduct };
    }

    public async Task<OutputVariantSetActive> SetVariantActiveAsync(InputVariantSetActive input, CancellationToken ct = default)
    {
        var entity = await _db.ProductVariants.FirstOrDefaultAsync(x => x.IdProductVariant == input.IdProductVariant && !x.Cancelled, ct);
        if (entity is null)
            return new OutputVariantSetActive { ResponseCode = 1, ResponseMsg = "Variant not found.", StatusCode = 404 };

        if (entity.IsActive != input.IsActive)
        {
            entity.IsActive = input.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return new OutputVariantSetActive
        {
            IdProductVariant = entity.IdProductVariant,
            FkProduct = entity.FkProduct,
            IsActive = entity.IsActive
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // PICKER LOOKUPS (JSON)
    // ═══════════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<OutputProductSearchItem>> SearchProductsAsync(InputProductSearch input, CancellationToken ct = default)
    {
        var take = Math.Clamp(input.Take, 1, 50);
        var term = (input.Term ?? string.Empty).Trim().ToLowerInvariant();
        var hasTerm = term.Length > 0;

        // Product rows. With ExpandVariants, a product that has active variants is represented by its variants only.
        var productQuery = _db.Products.Where(p => !p.Cancelled && p.IsActive);
        if (input.ExpandVariants)
            productQuery = productQuery.Where(p => !_db.ProductVariants.Any(v => v.FkProduct == p.IdProduct && !v.Cancelled && v.IsActive));
        if (hasTerm)
        {
            productQuery = productQuery.Where(p =>
                p.ProductName.ToLower().Contains(term) ||
                p.ProductCode.ToLower().Contains(term) ||
                (p.Sku != null && p.Sku.ToLower().Contains(term)) ||
                (p.Barcode != null && p.Barcode.ToLower().Contains(term)));
        }

        var products = await productQuery
            .OrderBy(p => p.ProductName).ThenBy(p => p.IdProduct)
            .Take(take)
            .Select(SearchItemFromProduct())
            .ToListAsync(ct);

        var variants = new List<OutputProductSearchItem>();
        if (input.ExpandVariants)
        {
            var variantQuery = _db.ProductVariants
                .Where(v => !v.Cancelled && v.IsActive && !v.Product!.Cancelled && v.Product!.IsActive);
            if (hasTerm)
            {
                variantQuery = variantQuery.Where(v =>
                    v.VariantName.ToLower().Contains(term) ||
                    (v.SkuCode != null && v.SkuCode.ToLower().Contains(term)) ||
                    (v.Barcode != null && v.Barcode.ToLower().Contains(term)) ||
                    v.Product!.ProductName.ToLower().Contains(term) ||
                    v.Product!.ProductCode.ToLower().Contains(term) ||
                    (v.Product!.Sku != null && v.Product!.Sku.ToLower().Contains(term)) ||
                    (v.Product!.Barcode != null && v.Product!.Barcode.ToLower().Contains(term)));
            }

            variants = await variantQuery
                .OrderBy(v => v.Product!.ProductName).ThenBy(v => v.VariantName).ThenBy(v => v.IdProductVariant)
                .Take(take)
                .Select(SearchItemFromVariant())
                .ToListAsync(ct);
        }

        // Exact barcode / SKU / code hits first (a scanner typing into the box), then alphabetical.
        return products
            .Concat(variants)
            .OrderBy(x => IsExactHit(x, term) ? 0 : 1)
            .ThenBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.VariantName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Take(take)
            .ToList();
    }

    public async Task<OutputProductSearchItem?> FindByBarcodeAsync(InputProductBarcode input, CancellationToken ct = default)
    {
        var code = (input.Barcode ?? string.Empty).Trim().ToLowerInvariant();
        if (code.Length == 0)
            return null;

        // Barcodes win over SKU codes; variants win over their parent product.
        var variant = await _db.ProductVariants
            .Where(v => !v.Cancelled && v.IsActive && !v.Product!.Cancelled && v.Product!.IsActive
                        && v.Barcode != null && v.Barcode.ToLower() == code)
            .Select(SearchItemFromVariant())
            .FirstOrDefaultAsync(ct);
        if (variant is not null)
            return variant;

        var product = await _db.Products
            .Where(p => !p.Cancelled && p.IsActive && p.Barcode != null && p.Barcode.ToLower() == code)
            .Select(SearchItemFromProduct())
            .FirstOrDefaultAsync(ct);
        if (product is not null)
            return product;

        variant = await _db.ProductVariants
            .Where(v => !v.Cancelled && v.IsActive && !v.Product!.Cancelled && v.Product!.IsActive
                        && v.SkuCode != null && v.SkuCode.ToLower() == code)
            .Select(SearchItemFromVariant())
            .FirstOrDefaultAsync(ct);
        if (variant is not null)
            return variant;

        return await _db.Products
            .Where(p => !p.Cancelled && p.IsActive && p.Sku != null && p.Sku.ToLower() == code)
            .Select(SearchItemFromProduct())
            .FirstOrDefaultAsync(ct);
    }

    // ─── projection helpers ───────────────────────────────────────

    private Expression<Func<Product, OutputProductSelect>> ProductProjection()
        => x => new OutputProductSelect
        {
            IdProduct = x.IdProduct,
            ProductCode = x.ProductCode,
            ProductName = x.ProductName,
            Description = x.Description,
            Sku = x.Sku,
            Barcode = x.Barcode,
            FkCategory = x.FkCategory,
            CategoryName = _db.Categories.Where(c => c.IdCategory == x.FkCategory).Select(c => c.CategoryName).FirstOrDefault(),
            FkSubcategory = x.FkSubcategory,
            SubCategoryName = _db.SubCategories.Where(s => s.IdSubcategory == x.FkSubcategory).Select(s => s.SubCategoryName).FirstOrDefault(),
            FkBrand = x.FkBrand,
            BrandName = _db.Brands.Where(b => b.IdBrand == x.FkBrand).Select(b => b.BrandName).FirstOrDefault(),
            FkUnit = x.FkUnit,
            UnitName = _db.Units.Where(u => u.IdUnit == x.FkUnit).Select(u => u.UnitName).FirstOrDefault(),
            FkTax = x.FkTax,
            TaxName = _db.Taxes.Where(t => t.IdTax == x.FkTax).Select(t => t.TaxName).FirstOrDefault(),
            TaxRate = _db.Taxes.Where(t => t.IdTax == x.FkTax).Select(t => (decimal?)t.TaxRate).FirstOrDefault() ?? 0m,
            TaxInclusive = _db.Taxes.Where(t => t.IdTax == x.FkTax).Select(t => (bool?)t.IsInclusive).FirstOrDefault() ?? false,
            CostPrice = x.CostPrice,
            SellingPrice = x.SellingPrice,
            Mrp = x.Mrp,
            ReorderLevel = x.ReorderLevel,
            IsActive = x.IsActive,
            IsOnline = x.IsOnline,
            VariantCount = _db.ProductVariants.Count(v => v.FkProduct == x.IdProduct && !v.Cancelled),
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };

    private static readonly Expression<Func<ProductVariant, OutputVariantSelect>> VariantProjection
        = x => new OutputVariantSelect
        {
            IdProductVariant = x.IdProductVariant,
            FkProduct = x.FkProduct,
            ProductCode = x.Product!.ProductCode,
            ProductName = x.Product!.ProductName,
            ProductIsActive = x.Product!.IsActive,
            ProductSellingPrice = x.Product!.SellingPrice,
            VariantName = x.VariantName,
            SkuCode = x.SkuCode,
            Barcode = x.Barcode,
            AdditionalPrice = x.AdditionalPrice,
            IsActive = x.IsActive
        };

    private Expression<Func<Product, OutputProductSearchItem>> SearchItemFromProduct()
        => p => new OutputProductSearchItem
        {
            IdProduct = p.IdProduct,
            IdProductVariant = null,
            ProductCode = p.ProductCode,
            ProductName = p.ProductName,
            VariantName = null,
            Sku = p.Sku,
            Barcode = p.Barcode,
            SellingPrice = p.SellingPrice,
            Mrp = p.Mrp,
            CostPrice = p.CostPrice,
            FkUnit = p.FkUnit,
            UnitName = _db.Units.Where(u => u.IdUnit == p.FkUnit).Select(u => u.UnitName).FirstOrDefault(),
            UnitCode = _db.Units.Where(u => u.IdUnit == p.FkUnit).Select(u => u.UnitCode).FirstOrDefault(),
            FkTax = p.FkTax,
            TaxRate = _db.Taxes.Where(t => t.IdTax == p.FkTax).Select(t => (decimal?)t.TaxRate).FirstOrDefault() ?? 0m,
            TaxInclusive = _db.Taxes.Where(t => t.IdTax == p.FkTax).Select(t => (bool?)t.IsInclusive).FirstOrDefault() ?? false,
            IsActive = p.IsActive
        };

    private Expression<Func<ProductVariant, OutputProductSearchItem>> SearchItemFromVariant()
        => v => new OutputProductSearchItem
        {
            IdProduct = v.FkProduct,
            IdProductVariant = v.IdProductVariant,
            ProductCode = v.Product!.ProductCode,
            ProductName = v.Product!.ProductName,
            VariantName = v.VariantName,
            Sku = v.SkuCode,
            Barcode = v.Barcode,
            SellingPrice = v.Product!.SellingPrice + v.AdditionalPrice,
            Mrp = v.Product!.Mrp + v.AdditionalPrice,
            CostPrice = v.Product!.CostPrice,
            FkUnit = v.Product!.FkUnit,
            UnitName = _db.Units.Where(u => u.IdUnit == v.Product!.FkUnit).Select(u => u.UnitName).FirstOrDefault(),
            UnitCode = _db.Units.Where(u => u.IdUnit == v.Product!.FkUnit).Select(u => u.UnitCode).FirstOrDefault(),
            FkTax = v.Product!.FkTax,
            TaxRate = _db.Taxes.Where(t => t.IdTax == v.Product!.FkTax).Select(t => (decimal?)t.TaxRate).FirstOrDefault() ?? 0m,
            TaxInclusive = _db.Taxes.Where(t => t.IdTax == v.Product!.FkTax).Select(t => (bool?)t.IsInclusive).FirstOrDefault() ?? false,
            IsActive = v.IsActive && v.Product!.IsActive
        };

    private static bool IsExactHit(OutputProductSearchItem item, string term)
        => term.Length > 0 && (
            string.Equals(item.Barcode, term, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(item.Sku, term, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(item.ProductCode, term, StringComparison.OrdinalIgnoreCase));

    // ─── constraint helpers ───────────────────────────────────────

    private static bool IsConstraintViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation };

    private static string ConstraintMessage(DbUpdateException ex)
    {
        if (ex.InnerException is not PostgresException pg)
            return "The change could not be saved.";

        if (pg.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            return "A selected category, sub-category, brand, unit or tax is no longer available.";

        return pg.ConstraintName switch
        {
            "uq_products_productcode" => "This product code is already in use.",
            "uq_products_sku" or "uq_product_variants_skucode" => "This SKU is already in use.",
            "uq_products_barcode" or "uq_product_variants_barcode" => "This barcode is already in use.",
            "uq_product_variants_product_name" => "A variant with this name already exists for the product.",
            _ => "This value is already in use."
        };
    }
}
