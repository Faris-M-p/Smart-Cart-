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

    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await LoadDropdownsAsync(ct);
        return View(new InProductCreateView { IsActive = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Create(InProductCreateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.CreateProductAsync(InputProductCreate.From(model), ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to create product.", result.ResponseMsg ?? "Unable to create product.");

        return CommonHelpers.SuccessResponse("Product created.", new { id = result.IdProduct });
    }

    // ═══════════════════════════════════════════════════════════════
    // EDIT
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await _svc.GetProductAsync(new InputProductSelect { IdProduct = id }, ct);
        if (item is null) return NotFound();
        await LoadDropdownsAsync(ct);
        return View(item.ToUpdateView());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Edit(int id, InProductUpdateView model, CancellationToken ct)
    {
        model.IdProduct = id;
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.UpdateProductAsync(InputProductUpdate.From(model), ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update product.", result.ResponseMsg ?? "Unable to update product.");

        return CommonHelpers.SuccessResponse("Product updated.", new { id = result.IdProduct });
    }

    // ═══════════════════════════════════════════════════════════════
    // VARIANTS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Variants(int productId, CancellationToken ct)
        => View(await _svc.ListVariantsAsync(new InputVariantList { FkProduct = productId }, ct));

    [HttpGet]
    [RequirePermission("products.manage")]
    public IActionResult CreateVariant(int productId)
        => View(new InVariantCreateView { FkProduct = productId, IsActive = true });

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateVariant(InVariantCreateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.CreateVariantAsync(InputVariantCreate.From(model), ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to create variant.", result.ResponseMsg ?? "Unable to create variant.");

        return CommonHelpers.SuccessResponse("Variant created.", new { id = result.IdProductVariant, productId = model.FkProduct });
    }

    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditVariant(int id, CancellationToken ct)
    {
        var item = await _svc.GetVariantAsync(new InputVariantSelect { IdProductVariant = id }, ct);
        if (item is null) return NotFound();
        return View(item.ToUpdateView());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditVariant(int id, InVariantUpdateView model, CancellationToken ct)
    {
        model.IdProductVariant = id;
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.UpdateVariantAsync(InputVariantUpdate.From(model), ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update variant.", result.ResponseMsg ?? "Unable to update variant.");

        return CommonHelpers.SuccessResponse("Variant updated.", new { id = result.IdProductVariant, productId = model.FkProduct });
    }

    // ═══════════════════════════════════════════════════════════════
    // BARCODE / SKU JSON LOOKUP
    // ═══════════════════════════════════════════════════════════════

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
