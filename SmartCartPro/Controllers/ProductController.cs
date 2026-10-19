using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>Product master CRUD + variant management.</summary>
[Authorize]
[ModuleAccess("PRODUCTS")]
public class ProductController : Controller
{
    private readonly IProductService _svc;
    private readonly ICatalogService _catalog;
    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    public ProductController(IProductService svc, ICatalogService catalog)
    {
        _svc = svc;
        _catalog = catalog;
    }

    // ═══════════════════════════════════════════════════════════════
    // PRODUCT LIST
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Lists store products with pricing, stock hints, and catalog links for maintenance.
    /// Staff with products.view open this as the main product master grid before POS sales.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Index([FromQuery] InputProductList input, CancellationToken ct)
    {
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.ListProductsAsync(input, ct));
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows the form to add a sellable SKU with barcode, prices, and catalog links.
    /// Managers with products.manage use it when onboarding items for register and online.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await LoadDropdownsAsync(ct);
        return View(new InProductCreateView { IsActive = true });
    }

    /// <summary>
    /// Persists a new product available for POS scan, inventory, and optional online sale.
    /// Managers submit the create product form after validating codes and tax assignment.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Create(InProductCreateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputProductCreate
        {
            ProductCode = model.ProductCode,
            ProductName = model.ProductName,
            Description = model.Description,
            Sku = model.Sku,
            Barcode = model.Barcode,
            FkCategory = model.FkCategory,
            FkSubcategory = model.FkSubcategory,
            FkBrand = model.FkBrand,
            FkUnit = model.FkUnit,
            FkTax = model.FkTax,
            CostPrice = model.CostPrice,
            SellingPrice = model.SellingPrice,
            Mrp = model.Mrp,
            ReorderLevel = model.ReorderLevel,
            IsActive = model.IsActive,
            IsOnline = model.IsOnline
        };
        var result = await _svc.CreateProductAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to create product.", result.ResponseMsg ?? "Unable to create product.");

        return CommonHelpers.SuccessResponse("Product created.", new { id = result.IdProduct });
    }

    // ═══════════════════════════════════════════════════════════════
    // EDIT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads one product for editing prices, barcode, category, and active or online flags.
    /// Managers open this from the product list when correcting shelf or register data.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await _svc.GetProductAsync(new InputProductSelect { IdProduct = id }, ct);
        if (item is null) return NotFound();
        await LoadDropdownsAsync(ct);
        return View(item.ToUpdateView());
    }

    /// <summary>
    /// Saves product changes reflected immediately on POS lookup and stock reports.
    /// Managers submit the edit product form after updating merchandising or pricing.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Edit(int id, InProductUpdateView model, CancellationToken ct)
    {
        model.IdProduct = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputProductUpdate
        {
            IdProduct = model.IdProduct,
            ProductCode = model.ProductCode,
            ProductName = model.ProductName,
            Description = model.Description,
            Sku = model.Sku,
            Barcode = model.Barcode,
            FkCategory = model.FkCategory,
            FkSubcategory = model.FkSubcategory,
            FkBrand = model.FkBrand,
            FkUnit = model.FkUnit,
            FkTax = model.FkTax,
            CostPrice = model.CostPrice,
            SellingPrice = model.SellingPrice,
            Mrp = model.Mrp,
            ReorderLevel = model.ReorderLevel,
            IsActive = model.IsActive,
            IsOnline = model.IsOnline
        };
        var result = await _svc.UpdateProductAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update product.", result.ResponseMsg ?? "Unable to update product.");

        return CommonHelpers.SuccessResponse("Product updated.", new { id = result.IdProduct });
    }

    // ═══════════════════════════════════════════════════════════════
    // VARIANTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Lists size, color, or other variants tied to a parent product for separate SKUs.
    /// Staff with products.view review variants before cashiers scan variant barcodes at POS.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Variants(int productId, CancellationToken ct)
        => View(await _svc.ListVariantsAsync(new InputVariantList { FkProduct = productId }, ct));

    /// <summary>
    /// Shows the form to add a variant with its own barcode and price adjustment.
    /// Managers with products.manage add variants for multi-SKU items like apparel sizes.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public IActionResult CreateVariant(int productId)
        => View(new InVariantCreateView { FkProduct = productId, IsActive = true });

    /// <summary>
    /// Saves a new variant scannable at checkout under the parent product.
    /// Managers submit the create variant form from the product variants screen.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateVariant(InVariantCreateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputVariantCreate
        {
            FkProduct = model.FkProduct,
            VariantName = model.VariantName,
            SkuCode = model.SkuCode,
            Barcode = model.Barcode,
            AdditionalPrice = model.AdditionalPrice,
            IsActive = model.IsActive
        };
        var result = await _svc.CreateVariantAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to create variant.", result.ResponseMsg ?? "Unable to create variant.");

        return CommonHelpers.SuccessResponse("Variant created.", new { id = result.IdProductVariant, productId = model.FkProduct });
    }

    /// <summary>
    /// Loads one variant for editing name, SKU, barcode, or supplemental price.
    /// Managers navigate here from the variant list when fixing register scan issues.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditVariant(int id, CancellationToken ct)
    {
        var item = await _svc.GetVariantAsync(new InputVariantSelect { IdProductVariant = id }, ct);
        if (item is null) return NotFound();
        return View(item.ToUpdateView());
    }

    /// <summary>
    /// Persists variant updates used when cashiers scan or search by variant barcode.
    /// Managers submit the edit variant form after revising sellable child SKUs.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditVariant(int id, InVariantUpdateView model, CancellationToken ct)
    {
        model.IdProductVariant = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputVariantUpdate
        {
            IdProductVariant = model.IdProductVariant,
            FkProduct = model.FkProduct,
            VariantName = model.VariantName,
            SkuCode = model.SkuCode,
            Barcode = model.Barcode,
            AdditionalPrice = model.AdditionalPrice,
            IsActive = model.IsActive
        };
        var result = await _svc.UpdateVariantAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update variant.", result.ResponseMsg ?? "Unable to update variant.");

        return CommonHelpers.SuccessResponse("Variant updated.", new { id = result.IdProductVariant, productId = model.FkProduct });
    }

    // ═══════════════════════════════════════════════════════════════
    // BARCODE / SKU JSON LOOKUP
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Resolves a scanned or keyed barcode to product or variant details as JSON.
    /// POS and receiving UIs call this during fast item lookup without a full page load.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> FindByBarcode(string barcode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return BadRequest();
        var result = await _svc.FindByBarcodeAsync(barcode, ct);
        if (result is null) return NotFound();
        return Json(result);
    }

    // ─── helpers ─────────────────────────────────────────────────

    private async Task LoadDropdownsAsync(CancellationToken ct)
    {
        ViewBag.Categories = await _catalog.GetCategoryLookupAsync(ct);
        ViewBag.Brands = await _catalog.GetBrandLookupAsync(ct);
        ViewBag.Units = await _catalog.GetUnitLookupAsync(ct);
        ViewBag.Taxes = await _catalog.GetTaxLookupAsync(ct);
    }
}
