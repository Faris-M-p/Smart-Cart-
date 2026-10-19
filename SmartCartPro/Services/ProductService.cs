using SmartCartPro.Exceptions;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class ProductService : IProductService
{
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;
    private const int MaxTermLength = 100;

    /// <summary>Variants shown on the product detail page (the full list lives on the variants screen).</summary>
    private const int DetailVariantLimit = 100;

    private static readonly string[] ProductSorts = { "productname", "productcode", "category", "sellingprice", "isactive" };
    private static readonly string[] VariantSorts = { "variantname", "skucode", "additionalprice", "isactive", "product" };

    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo) => _repo = repo;

    // ═══════════════════════════════════════════════════════════════
    // PRODUCT
    // ═══════════════════════════════════════════════════════════════

    public Task<OutputProductSelect?> GetProductAsync(InputProductSelect input, CancellationToken ct = default)
        => _repo.GetProductAsync(input, ct);

    public async Task<OutputProductDetail?> GetProductDetailAsync(InputProductDetail input, CancellationToken ct = default)
    {
        var selectInput = new InputProductSelect { IdProduct = input.IdProduct };
        var product = await _repo.GetProductAsync(selectInput, ct);
        if (product is null)
            return null;

        var variantInput = new InputVariantList
        {
            FkProduct = input.IdProduct,
            Page = 1,
            PageSize = DetailVariantLimit,
            SortBy = "variantname"
        };
        var variants = await _repo.ListVariantsAsync(variantInput, ct);

        var detail = new OutputProductDetail
        {
            Product = product,
            Variants = variants.Items,
            VariantTotal = variants.TotalCount
        };

        if (input.FkStore > 0)
        {
            var stockInput = new InputProductStockList { FkStore = input.FkStore, FkProduct = input.IdProduct };
            var stock = await _repo.ListProductStockAsync(stockInput, ct);
            detail.Stock = stock.ToList();
            detail.HasStock = true;
        }

        return detail;
    }

    public async Task<OutputProductList> ListProductsAsync(InputProductList input, CancellationToken ct = default)
    {
        input.Search = Truncate(Clean(input.Search), MaxTermLength);
        input.Page = Math.Max(1, input.Page);
        input.PageSize = input.PageSize is < 1 or > MaxPageSize ? DefaultPageSize : input.PageSize;
        input.FkCategory = Fk(input.FkCategory);
        input.FkSubcategory = Fk(input.FkSubcategory);
        input.FkBrand = Fk(input.FkBrand);
        input.SortBy = NormalizeSort(input.SortBy, ProductSorts, "productname");

        var lookupInput = new InputProductLookup
        {
            IncludeInactive = true,
            FkCategory = input.FkCategory,
            FkSubcategory = input.FkSubcategory,
            FkBrand = input.FkBrand,
            IncludeUnitsAndTaxes = false
        };
        var lookup = await _repo.GetProductLookupAsync(lookupInput, ct);

        // A sub-category filter only makes sense inside its category (the user may have just changed the category).
        if (input.FkSubcategory.HasValue && lookup.SubCategories.All(s => s.Id != input.FkSubcategory.Value))
            input.FkSubcategory = null;

        var list = await _repo.ListProductsAsync(input, ct);
        list.Lookup = lookup;
        return list;
    }

    public async Task<OutputProductCreate> CreateProductAsync(InputProductCreate input, CancellationToken ct = default)
    {
        input.ProductCode = (input.ProductCode ?? string.Empty).Trim();
        input.ProductName = (input.ProductName ?? string.Empty).Trim();
        input.Description = Clean(input.Description);
        input.Sku = Clean(input.Sku);
        input.Barcode = Clean(input.Barcode);
        input.FkCategory = Fk(input.FkCategory);
        input.FkSubcategory = Fk(input.FkSubcategory);
        input.FkBrand = Fk(input.FkBrand);
        input.FkUnit = Fk(input.FkUnit);
        input.FkTax = Fk(input.FkTax);
        input.CostPrice = Money(input.CostPrice);
        input.SellingPrice = Money(input.SellingPrice);
        input.Mrp = Money(input.Mrp);
        input.ReorderLevel = Quantity(input.ReorderLevel);

        var checkInput = new InputProductCheck
        {
            IdProduct = 0,
            ProductCode = input.ProductCode,
            Sku = input.Sku,
            Barcode = input.Barcode,
            FkCategory = input.FkCategory,
            FkSubcategory = input.FkSubcategory,
            FkBrand = input.FkBrand,
            FkUnit = input.FkUnit,
            FkTax = input.FkTax
        };
        var check = await _repo.CheckProductAsync(checkInput, ct);
        ThrowIfInvalid(check, updating: false);

        var result = await _repo.CreateProductAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to create product.");

        return result;
    }

    public async Task<OutputProductUpdate> UpdateProductAsync(InputProductUpdate input, CancellationToken ct = default)
    {
        input.ProductCode = (input.ProductCode ?? string.Empty).Trim();
        input.ProductName = (input.ProductName ?? string.Empty).Trim();
        input.Description = Clean(input.Description);
        input.Sku = Clean(input.Sku);
        input.Barcode = Clean(input.Barcode);
        input.FkCategory = Fk(input.FkCategory);
        input.FkSubcategory = Fk(input.FkSubcategory);
        input.FkBrand = Fk(input.FkBrand);
        input.FkUnit = Fk(input.FkUnit);
        input.FkTax = Fk(input.FkTax);
        input.CostPrice = Money(input.CostPrice);
        input.SellingPrice = Money(input.SellingPrice);
        input.Mrp = Money(input.Mrp);
        input.ReorderLevel = Quantity(input.ReorderLevel);

        var checkInput = new InputProductCheck
        {
            IdProduct = input.IdProduct,
            ProductCode = input.ProductCode,
            Sku = input.Sku,
            Barcode = input.Barcode,
            FkCategory = input.FkCategory,
            FkSubcategory = input.FkSubcategory,
            FkBrand = input.FkBrand,
            FkUnit = input.FkUnit,
            FkTax = input.FkTax
        };
        var check = await _repo.CheckProductAsync(checkInput, ct);
        ThrowIfInvalid(check, updating: true);

        var result = await _repo.UpdateProductAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to update product.");

        return result;
    }

    public async Task<OutputProductSetActive> SetProductActiveAsync(InputProductSetActive input, CancellationToken ct = default)
    {
        var result = await _repo.SetProductActiveAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to change the product status.");

        return result;
    }

    public Task<OutputProductLookup> GetProductLookupAsync(InputProductLookup input, CancellationToken ct = default)
        => _repo.GetProductLookupAsync(input, ct);

    // ═══════════════════════════════════════════════════════════════
    // VARIANT
    // ═══════════════════════════════════════════════════════════════

    public Task<OutputVariantSelect?> GetVariantAsync(InputVariantSelect input, CancellationToken ct = default)
        => _repo.GetVariantAsync(input, ct);

    public async Task<OutputVariantList?> ListVariantsAsync(InputVariantList input, CancellationToken ct = default)
    {
        input.FkProduct = Fk(input.FkProduct);
        input.Search = Truncate(Clean(input.Search), MaxTermLength);
        input.Page = Math.Max(1, input.Page);
        input.PageSize = input.PageSize is < 1 or > MaxPageSize ? DefaultPageSize : input.PageSize;
        input.SortBy = NormalizeSort(input.SortBy, VariantSorts, input.FkProduct.HasValue ? "variantname" : "product");

        var list = await _repo.ListVariantsAsync(input, ct);
        if (input.FkProduct.HasValue && string.IsNullOrEmpty(list.ProductName))
            return null;

        return list;
    }

    public async Task<OutputVariantCreate> CreateVariantAsync(InputVariantCreate input, CancellationToken ct = default)
    {
        input.VariantName = (input.VariantName ?? string.Empty).Trim();
        input.SkuCode = Clean(input.SkuCode);
        input.Barcode = Clean(input.Barcode);
        input.AdditionalPrice = Money(input.AdditionalPrice);

        var checkInput = new InputVariantCheck
        {
            IdProductVariant = 0,
            FkProduct = input.FkProduct,
            VariantName = input.VariantName,
            SkuCode = input.SkuCode,
            Barcode = input.Barcode
        };
        var check = await _repo.CheckVariantAsync(checkInput, ct);
        ThrowIfInvalid(check, updating: false);

        var result = await _repo.CreateVariantAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to create variant.");

        return result;
    }

    public async Task<OutputVariantUpdate> UpdateVariantAsync(InputVariantUpdate input, CancellationToken ct = default)
    {
        input.VariantName = (input.VariantName ?? string.Empty).Trim();
        input.SkuCode = Clean(input.SkuCode);
        input.Barcode = Clean(input.Barcode);
        input.AdditionalPrice = Money(input.AdditionalPrice);

        var checkInput = new InputVariantCheck
        {
            IdProductVariant = input.IdProductVariant,
            FkProduct = input.FkProduct,
            VariantName = input.VariantName,
            SkuCode = input.SkuCode,
            Barcode = input.Barcode
        };
        var check = await _repo.CheckVariantAsync(checkInput, ct);
        ThrowIfInvalid(check, updating: true);

        // The parent product is never taken from the client: the stored one wins.
        input.FkProduct = check.FkProduct;

        var result = await _repo.UpdateVariantAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to update variant.");

        return result;
    }

    public async Task<OutputVariantSetActive> SetVariantActiveAsync(InputVariantSetActive input, CancellationToken ct = default)
    {
        var result = await _repo.SetVariantActiveAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to change the variant status.");

        return result;
    }

    // ═══════════════════════════════════════════════════════════════
    // PICKER LOOKUPS (JSON)
    // ═══════════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<OutputProductSearchItem>> SearchProductsAsync(InputProductSearch input, CancellationToken ct = default)
    {
        input.Term = Truncate(Clean(input.Term), MaxTermLength);
        input.Take = Math.Clamp(input.Take, 1, 50);

        var items = await _repo.SearchProductsAsync(input, ct);
        if (!input.IncludeCost)
        {
            foreach (var item in items)
                item.CostPrice = null;
        }

        return items;
    }

    public async Task<OutputProductSearchItem?> FindByBarcodeAsync(InputProductBarcode input, CancellationToken ct = default)
    {
        input.Barcode = (input.Barcode ?? string.Empty).Trim();
        if (input.Barcode.Length == 0 || input.Barcode.Length > MaxTermLength)
            return null;

        var item = await _repo.FindByBarcodeAsync(input, ct);
        if (item is not null && !input.IncludeCost)
            item.CostPrice = null;

        return item;
    }

    // ─── validation helpers ───────────────────────────────────────

    private static void ThrowIfInvalid(OutputProductCheck check, bool updating)
    {
        if (updating && !check.ProductExists)
            throw new ResourceNotFoundException("Product not found.", "The product does not exist or is cancelled.");

        var errors = new Dictionary<string, string[]>();

        if (check.CodeInUse)
            errors["ProductCode"] = new[] { "This product code is already in use." };
        if (check.SkuInUse)
            errors["Sku"] = new[] { "This SKU is already used by another product or variant." };
        if (check.BarcodeInUse)
            errors["Barcode"] = new[] { "This barcode is already used by another product or variant." };
        if (check.CategoryMissing)
            errors["FkCategory"] = new[] { "The selected category is no longer available." };
        if (check.SubCategoryMissing)
            errors["FkSubcategory"] = new[] { "The selected sub-category is no longer available." };
        else if (check.SubCategoryOutsideCategory)
            errors["FkSubcategory"] = new[] { "The sub-category must belong to the selected category." };
        if (check.BrandMissing)
            errors["FkBrand"] = new[] { "The selected brand is no longer available." };
        if (check.UnitMissing)
            errors["FkUnit"] = new[] { "The selected unit is no longer available." };
        if (check.TaxMissing)
            errors["FkTax"] = new[] { "The selected tax is no longer available." };

        ThrowIfAny(errors, "Product validation failed.");
    }

    private static void ThrowIfInvalid(OutputVariantCheck check, bool updating)
    {
        if (updating && !check.VariantExists)
            throw new ResourceNotFoundException("Variant not found.", "The variant does not exist or is cancelled.");
        if (!check.ProductExists)
            throw new ResourceNotFoundException("Product not found.", "The parent product does not exist or is cancelled.");

        var errors = new Dictionary<string, string[]>();

        if (check.NameInUse)
            errors["VariantName"] = new[] { "A variant with this name already exists for the product." };
        if (check.SkuInUse)
            errors["SkuCode"] = new[] { "This SKU code is already used by another variant or product." };
        if (check.BarcodeInUse)
            errors["Barcode"] = new[] { "This barcode is already used by another variant or product." };

        ThrowIfAny(errors, "Variant validation failed.");
    }

    private static void ThrowIfAny(Dictionary<string, string[]> errors, string developerMessage)
    {
        if (errors.Count == 0)
            return;

        var summary = string.Join(" ", errors.Values.SelectMany(messages => messages));
        throw new ValidationException(summary, developerMessage, errors);
    }

    /// <summary>Maps a failed repository result (404 / 409 / other) to the matching typed exception.</summary>
    private static AppException Failure(int statusCode, string? message, string fallback)
    {
        var text = string.IsNullOrWhiteSpace(message) ? fallback : message;
        return statusCode switch
        {
            404 => new ResourceNotFoundException(text, text),
            409 => new ConflictException(text, text),
            _ => new UpdateFailedException(fallback, text)
        };
    }

    // ─── normalisation helpers ────────────────────────────────────

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Truncate(string? value, int max)
        => value is not null && value.Length > max ? value[..max] : value;

    /// <summary>Dropdown "none" arrives as 0 or null; both mean no reference.</summary>
    private static int? Fk(int? id)
        => id is > 0 ? id : null;

    private static decimal Money(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal? Money(decimal? value)
        => value.HasValue ? Money(value.Value) : null;

    private static decimal Quantity(decimal value)
        => Math.Round(value, 3, MidpointRounding.AwayFromZero);

    private static string NormalizeSort(string? value, string[] allowed, string fallback)
    {
        var key = (value ?? string.Empty).Trim().ToLowerInvariant();
        return Array.IndexOf(allowed, key) >= 0 ? key : fallback;
    }
}
