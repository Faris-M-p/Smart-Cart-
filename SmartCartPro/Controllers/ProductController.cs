using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>Product master and variants (SKU): list, detail, create/edit, activate and picker lookups.</summary>
[Authorize]
[ModuleAccess("PRODUCTS")]
public class ProductController : Controller
{
    private readonly IProductService _svc;

    public ProductController(IProductService svc) => _svc = svc;

    // ═══════════════════════════════════════════════════════════════
    // PRODUCT LIST  (views live in ~/Views/Product/)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Lists products with search, category/brand/status filters, sorting and paging.
    /// Staff with products.view open this as the main product master grid.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Index([FromQuery] InputProductList input, CancellationToken ct)
    {
        var model = await _svc.ListProductsAsync(input, ct);
        return View("~/Views/Product/Products.cshtml", model);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRODUCT DETAIL
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows one product with its variants and, for store users, the stock on hand.
    /// Staff open it from the product list to review pricing before adding variants.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Details(int id, string? returnUrl, CancellationToken ct)
    {
        var input = new InputProductDetail
        {
            IdProduct = id,
            FkStore = AuthClaims.GetFkStore(User) ?? 0
        };
        var detail = await _svc.GetProductDetailAsync(input, ct);
        if (detail is null)
            return NotFound();

        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View("~/Views/Product/ProductDetails.cshtml", detail);
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows the form to add a sellable item with barcode, prices and catalog links.
    /// Managers with products.manage use it when onboarding items for register and online.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Create(string? returnUrl, CancellationToken ct)
    {
        var lookupInput = new InputProductLookup();
        await PrepareFormAsync(lookupInput, returnUrl, ct);
        return View("~/Views/Product/CreateProduct.cshtml", new InProductCreateView { IsActive = true });
    }

    /// <summary>
    /// Saves a new product available for POS scan, inventory and optional online sale.
    /// Managers submit the create product form after validating codes and tax assignment.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Create(InProductCreateView model, string? returnUrl, CancellationToken ct)
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

        // New products land on their detail page so variants can be added right away.
        var detailUrl = Url.Action(nameof(Details), new { id = result.IdProduct, returnUrl = SafeReturnUrl(returnUrl) })!;
        return CommonHelpers.SuccessResponse(
            "Product created successfully.",
            new { id = result.IdProduct, redirectUrl = detailUrl });
    }

    // ═══════════════════════════════════════════════════════════════
    // EDIT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Loads one product for editing prices, barcode, category and active or online flags.
    /// Managers open this from the product list or detail page when correcting item data.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Edit(int id, string? returnUrl, CancellationToken ct)
    {
        var selectInput = new InputProductSelect { IdProduct = id };
        var item = await _svc.GetProductAsync(selectInput, ct);
        if (item is null)
            return NotFound();

        var lookupInput = new InputProductLookup
        {
            FkCategory = item.FkCategory,
            FkSubcategory = item.FkSubcategory,
            FkBrand = item.FkBrand,
            FkUnit = item.FkUnit,
            FkTax = item.FkTax
        };
        await PrepareFormAsync(lookupInput, returnUrl, ct);
        return View("~/Views/Product/EditProduct.cshtml", item.ToUpdateView());
    }

    /// <summary>
    /// Saves product changes reflected immediately on POS lookup and stock reports.
    /// Managers submit the edit product form after updating merchandising or pricing.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Edit(int id, InProductUpdateView model, string? returnUrl, CancellationToken ct)
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

        var nextUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Details), new { id = result.IdProduct })!;
        return CommonHelpers.SuccessResponse(
            "Product updated successfully.",
            new { id = result.IdProduct, redirectUrl = nextUrl });
    }

    /// <summary>
    /// Activates or deactivates a product from the list or detail page without opening the form.
    /// Managers with products.manage use this after confirming the status change.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> SetActive(int id, bool isActive, CancellationToken ct)
    {
        var input = new InputProductSetActive { IdProduct = id, IsActive = isActive };
        var result = await _svc.SetProductActiveAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            result.IsActive ? "Product activated." : "Product deactivated.",
            new { id = result.IdProduct, isActive = result.IsActive });
    }

    // ═══════════════════════════════════════════════════════════════
    // VARIANTS  (views live in ~/Views/ProductVariant/)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Lists variants (SKUs) of one product, or of every product when no product is chosen.
    /// Staff with products.view use it to check SKU codes, barcodes and price adjustments.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Variants([FromQuery] InputVariantList input, int? productId, CancellationToken ct)
    {
        if (productId is > 0)
            input.FkProduct = productId;

        var model = await _svc.ListVariantsAsync(input, ct);
        if (model is null)
            return NotFound();

        return View("~/Views/ProductVariant/Variants.cshtml", model);
    }

    /// <summary>
    /// Shows the form to add a variant with its own SKU, barcode and price adjustment.
    /// Managers with products.manage add variants for multi-SKU items like apparel sizes.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateVariant(int productId, string? returnUrl, CancellationToken ct)
    {
        var selectInput = new InputProductSelect { IdProduct = productId };
        var product = await _svc.GetProductAsync(selectInput, ct);
        if (product is null)
            return NotFound();

        ViewBag.Product = product;
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(
            "~/Views/ProductVariant/CreateVariant.cshtml",
            new InVariantCreateView { FkProduct = product.IdProduct, IsActive = true });
    }

    /// <summary>
    /// Saves a new variant that can be scanned at checkout under its parent product.
    /// Managers submit the create variant form from the product variants screen.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateVariant(InVariantCreateView model, string? returnUrl, CancellationToken ct)
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

        var nextUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Variants), new { productId = result.FkProduct })!;
        return CommonHelpers.SuccessResponse(
            "Variant created successfully.",
            new { id = result.IdProductVariant, productId = result.FkProduct, redirectUrl = nextUrl });
    }

    /// <summary>
    /// Loads one variant for editing name, SKU, barcode or additional price.
    /// Managers navigate here from the variant list when fixing register scan issues.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditVariant(int id, string? returnUrl, CancellationToken ct)
    {
        var selectInput = new InputVariantSelect { IdProductVariant = id };
        var item = await _svc.GetVariantAsync(selectInput, ct);
        if (item is null)
            return NotFound();

        ViewBag.Variant = item;
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View("~/Views/ProductVariant/EditVariant.cshtml", item.ToUpdateView());
    }

    /// <summary>
    /// Saves variant changes used when cashiers scan or search by variant barcode.
    /// Managers submit the edit variant form after revising a sellable child SKU.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditVariant(int id, InVariantUpdateView model, string? returnUrl, CancellationToken ct)
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

        var nextUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Variants), new { productId = result.FkProduct })!;
        return CommonHelpers.SuccessResponse(
            "Variant updated successfully.",
            new { id = result.IdProductVariant, productId = result.FkProduct, redirectUrl = nextUrl });
    }

    /// <summary>
    /// Activates or deactivates a variant from a list without opening the edit form.
    /// Managers with products.manage use this after confirming the status change.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> SetVariantActive(int id, bool isActive, CancellationToken ct)
    {
        var input = new InputVariantSetActive { IdProductVariant = id, IsActive = isActive };
        var result = await _svc.SetVariantActiveAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            result.IsActive ? "Variant activated." : "Variant deactivated.",
            new { id = result.IdProductVariant, productId = result.FkProduct, isActive = result.IsActive });
    }

    // ═══════════════════════════════════════════════════════════════
    // PICKER LOOKUPS (JSON)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Searches active products and variants by name, code, SKU or barcode as JSON for typeahead pickers.
    /// Purchase, stock and POS screens call this while the user types; the rows sit in data.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search(string? term, int take = 20, bool expandVariants = true, CancellationToken ct = default)
    {
        var input = new InputProductSearch
        {
            Term = term,
            Take = take,
            ExpandVariants = expandVariants
        };
        var items = await _svc.SearchProductsAsync(input, ct);

        return CommonHelpers.SuccessResponse("OK", items);
    }

    /// <summary>
    /// Resolves a scanned or keyed barcode (or SKU) to one product or variant as a plain JSON object.
    /// POS and receiving UIs call this during fast item lookup; 404 means no active match.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> FindByBarcode(string barcode, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return BadRequest();

        var input = new InputProductBarcode { Barcode = barcode };
        var item = await _svc.FindByBarcodeAsync(input, ct);
        if (item is null)
            return NotFound();

        return Json(item);
    }

    // ─── helpers ─────────────────────────────────────────────────

    /// <summary>Loads the dropdown data and the safe return link the product form views read from ViewBag.</summary>
    private async Task PrepareFormAsync(InputProductLookup lookupInput, string? returnUrl, CancellationToken ct)
    {
        ViewBag.Lookup = await _svc.GetProductLookupAsync(lookupInput, ct);
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
    }

    private string? SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return null;
        if (!Url.IsLocalUrl(returnUrl))
            return null;
        return returnUrl;
    }
}
