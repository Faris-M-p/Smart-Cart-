using Microsoft.EntityFrameworkCore;
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
        if (input.ActiveOnly == true) q = q.Where(x => x.IsActive);
        if (input.FkCategory.HasValue) q = q.Where(x => x.FkCategory == input.FkCategory.Value);
        if (input.FkBrand.HasValue) q = q.Where(x => x.FkBrand == input.FkBrand.Value);
        if (!string.IsNullOrWhiteSpace(input.Search))
            q = q.Where(x =>
                x.ProductName.Contains(input.Search) ||
                x.ProductCode.Contains(input.Search) ||
                (x.Sku != null && x.Sku.Contains(input.Search)) ||
                (x.Barcode != null && x.Barcode.Contains(input.Search)));

        q = input.SortBy.ToLowerInvariant() switch
        {
            "productcode" => input.SortDesc ? q.OrderByDescending(x => x.ProductCode) : q.OrderBy(x => x.ProductCode),
            "sellingprice" => input.SortDesc ? q.OrderByDescending(x => x.SellingPrice) : q.OrderBy(x => x.SellingPrice),
            _ => input.SortDesc ? q.OrderByDescending(x => x.ProductName) : q.OrderBy(x => x.ProductName)
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(ProductProjection())
            .ToListAsync(ct);

        return new OutputProductList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
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
        await _db.SaveChangesAsync(ct);
        return new OutputProductCreate { IdProduct = entity.IdProduct, ProductCode = entity.ProductCode, ProductName = entity.ProductName };
    }

    public async Task<OutputProductUpdate> UpdateProductAsync(InputProductUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(x => x.IdProduct == input.IdProduct && !x.Cancelled, ct);
        if (entity is null) return new OutputProductUpdate { ResponseCode = 1, ResponseMsg = "Product not found.", StatusCode = 404 };

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

        await _db.SaveChangesAsync(ct);
        return new OutputProductUpdate { IdProduct = entity.IdProduct };
    }

    // ═══════════════════════════════════════════════════════════════
    // VARIANT
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputVariantSelect?> GetVariantAsync(InputVariantSelect input, CancellationToken ct = default)
        => await _db.ProductVariants
            .Where(x => x.IdProductVariant == input.IdProductVariant && !x.Cancelled)
            .Select(x => new OutputVariantSelect
            {
                IdProductVariant = x.IdProductVariant,
                FkProduct = x.FkProduct,
                ProductName = x.Product != null ? x.Product.ProductName : string.Empty,
                VariantName = x.VariantName,
                SkuCode = x.SkuCode,
                Barcode = x.Barcode,
                AdditionalPrice = x.AdditionalPrice,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);

    public async Task<OutputVariantList> ListVariantsAsync(InputVariantList input, CancellationToken ct = default)
    {
        var q = _db.ProductVariants
            .Include(x => x.Product)
            .Where(x => x.FkProduct == input.FkProduct && !x.Cancelled);
        if (input.ActiveOnly == true) q = q.Where(x => x.IsActive);

        var productName = await _db.Products.Where(x => x.IdProduct == input.FkProduct).Select(x => x.ProductName).FirstOrDefaultAsync(ct) ?? string.Empty;

        var items = await q.OrderBy(x => x.VariantName)
            .Select(x => new OutputVariantSelect
            {
                IdProductVariant = x.IdProductVariant,
                FkProduct = x.FkProduct,
                ProductName = x.Product != null ? x.Product.ProductName : string.Empty,
                VariantName = x.VariantName,
                SkuCode = x.SkuCode,
                Barcode = x.Barcode,
                AdditionalPrice = x.AdditionalPrice,
                IsActive = x.IsActive
            })
            .ToListAsync(ct);

        return new OutputVariantList { FkProduct = input.FkProduct, ProductName = productName, Items = items };
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
        await _db.SaveChangesAsync(ct);
        return new OutputVariantCreate { IdProductVariant = entity.IdProductVariant };
    }

    public async Task<OutputVariantUpdate> UpdateVariantAsync(InputVariantUpdate input, CancellationToken ct = default)
    {
        var entity = await _db.ProductVariants.FirstOrDefaultAsync(x => x.IdProductVariant == input.IdProductVariant && !x.Cancelled, ct);
        if (entity is null) return new OutputVariantUpdate { ResponseCode = 1, ResponseMsg = "Variant not found.", StatusCode = 404 };

        entity.VariantName = input.VariantName;
        entity.SkuCode = input.SkuCode;
        entity.Barcode = input.Barcode;
        entity.AdditionalPrice = input.AdditionalPrice;
        entity.IsActive = input.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return new OutputVariantUpdate { IdProductVariant = entity.IdProductVariant };
    }

    // ═══════════════════════════════════════════════════════════════
    // BARCODE LOOKUP
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputProductSelect?> FindByBarcodeAsync(string barcode, CancellationToken ct = default)
        => await _db.Products
            .Where(x => !x.Cancelled && x.IsActive && (x.Barcode == barcode || x.Sku == barcode))
            .Select(ProductProjection())
            .FirstOrDefaultAsync(ct);

    // ─── projection helper ────────────────────────────────────────

    private static System.Linq.Expressions.Expression<Func<Product, OutputProductSelect>> ProductProjection()
        => x => new OutputProductSelect
        {
            IdProduct = x.IdProduct,
            ProductCode = x.ProductCode,
            ProductName = x.ProductName,
            Description = x.Description,
            Sku = x.Sku,
            Barcode = x.Barcode,
            FkCategory = x.FkCategory,
            FkSubcategory = x.FkSubcategory,
            FkBrand = x.FkBrand,
            FkUnit = x.FkUnit,
            FkTax = x.FkTax,
            CostPrice = x.CostPrice,
            SellingPrice = x.SellingPrice,
            Mrp = x.Mrp,
            ReorderLevel = x.ReorderLevel,
            IsActive = x.IsActive,
            IsOnline = x.IsOnline
        };
}
