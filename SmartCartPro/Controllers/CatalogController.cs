using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>Catalog masters: Category, SubCategory, Brand, Unit, Tax.</summary>
[Authorize]
[ModuleAccess("PRODUCTS")]
public class CatalogController : Controller
{
    private readonly ICatalogService _svc;
    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    public CatalogController(ICatalogService svc) => _svc = svc;

    // ═══════════════════════════════════════════════════════════════
    // CATEGORY
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Categories([FromQuery] InputCategoryList input, CancellationToken ct)
    {
        input.Page = Math.Max(1, input.Page);
        var model = await _svc.ListCategoriesAsync(input, ct);
        return View(model);
    }

    [HttpGet]
    [RequirePermission("products.manage")]
    public IActionResult CreateCategory() => View(new InCategoryCreateView { IsActive = true });

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateCategory(InCategoryCreateView model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _svc.CreateCategoryAsync(InputCategoryCreate.From(model), ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = "Category created.";
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditCategory(int id, CancellationToken ct)
    {
        var item = await _svc.GetCategoryAsync(new InputCategorySelect { IdCategory = id }, ct);
        if (item is null) return NotFound();
        return View(item.ToUpdateView());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditCategory(int id, InCategoryUpdateView model, CancellationToken ct)
    {
        model.IdCategory = id;
        if (!ModelState.IsValid) return View(model);
        var result = await _svc.UpdateCategoryAsync(InputCategoryUpdate.From(model), ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = "Category updated.";
        return RedirectToAction(nameof(Categories));
    }

    // ─── JSON Lookup ──────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> CategoryLookup(CancellationToken ct)
        => Json(await _svc.GetCategoryLookupAsync(ct));

    [HttpGet]
    public async Task<IActionResult> SubCategoryLookup(int fkCategory, CancellationToken ct)
        => Json(await _svc.GetSubCategoryLookupAsync(fkCategory, ct));

    // ═══════════════════════════════════════════════════════════════
    // SUB-CATEGORY
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> SubCategories([FromQuery] InputSubCategoryList input, CancellationToken ct)
    {
        input.Page = Math.Max(1, input.Page);
        var model = await _svc.ListSubCategoriesAsync(input, ct);
        return View(model);
    }

    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateSubCategory(CancellationToken ct)
    {
        ViewBag.Categories = await _svc.GetCategoryLookupAsync(ct);
        return View(new InSubCategoryCreateView { IsActive = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateSubCategory(InSubCategoryCreateView model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _svc.GetCategoryLookupAsync(ct);
            return View(model);
        }
        var result = await _svc.CreateSubCategoryAsync(InputSubCategoryCreate.From(model), ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.ResponseMsg);
            ViewBag.Categories = await _svc.GetCategoryLookupAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "Sub-category created.";
        return RedirectToAction(nameof(SubCategories));
    }

    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditSubCategory(int id, CancellationToken ct)
    {
        var item = await _svc.GetSubCategoryAsync(new InputSubCategorySelect { IdSubCategory = id }, ct);
        if (item is null) return NotFound();
        ViewBag.Categories = await _svc.GetCategoryLookupAsync(ct);
        return View(item.ToUpdateView());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditSubCategory(int id, InSubCategoryUpdateView model, CancellationToken ct)
    {
        model.IdSubCategory = id;
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _svc.GetCategoryLookupAsync(ct);
            return View(model);
        }
        var result = await _svc.UpdateSubCategoryAsync(InputSubCategoryUpdate.From(model), ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.ResponseMsg);
            ViewBag.Categories = await _svc.GetCategoryLookupAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "Sub-category updated.";
        return RedirectToAction(nameof(SubCategories));
    }

    // ═══════════════════════════════════════════════════════════════
    // BRAND
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Brands([FromQuery] InputBrandList input, CancellationToken ct)
    {
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.ListBrandsAsync(input, ct));
    }

    [HttpGet]
    [RequirePermission("products.manage")]
    public IActionResult CreateBrand() => View(new InBrandCreateView { IsActive = true });

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateBrand(InBrandCreateView model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _svc.CreateBrandAsync(InputBrandCreate.From(model), ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = "Brand created.";
        return RedirectToAction(nameof(Brands));
    }

    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditBrand(int id, CancellationToken ct)
    {
        var item = await _svc.GetBrandAsync(new InputBrandSelect { IdBrand = id }, ct);
        if (item is null) return NotFound();
        return View(item.ToUpdateView());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditBrand(int id, InBrandUpdateView model, CancellationToken ct)
    {
        model.IdBrand = id;
        if (!ModelState.IsValid) return View(model);
        var result = await _svc.UpdateBrandAsync(InputBrandUpdate.From(model), ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = "Brand updated.";
        return RedirectToAction(nameof(Brands));
    }

    [HttpGet]
    public async Task<IActionResult> BrandLookup(CancellationToken ct)
        => Json(await _svc.GetBrandLookupAsync(ct));

    // ═══════════════════════════════════════════════════════════════
    // UNIT
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Units([FromQuery] InputUnitList input, CancellationToken ct)
    {
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.ListUnitsAsync(input, ct));
    }

    [HttpGet]
    [RequirePermission("products.manage")]
    public IActionResult CreateUnit() => View(new InUnitCreateView { IsActive = true });

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateUnit(InUnitCreateView model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _svc.CreateUnitAsync(InputUnitCreate.From(model), ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = "Unit created.";
        return RedirectToAction(nameof(Units));
    }

    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditUnit(int id, CancellationToken ct)
    {
        var item = await _svc.GetUnitAsync(new InputUnitSelect { IdUnit = id }, ct);
        if (item is null) return NotFound();
        return View(item.ToUpdateView());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditUnit(int id, InUnitUpdateView model, CancellationToken ct)
    {
        model.IdUnit = id;
        if (!ModelState.IsValid) return View(model);
        var result = await _svc.UpdateUnitAsync(InputUnitUpdate.From(model), ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = "Unit updated.";
        return RedirectToAction(nameof(Units));
    }

    [HttpGet]
    public async Task<IActionResult> UnitLookup(CancellationToken ct)
        => Json(await _svc.GetUnitLookupAsync(ct));

    // ═══════════════════════════════════════════════════════════════
    // TAX
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Taxes([FromQuery] InputTaxList input, CancellationToken ct)
    {
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.ListTaxesAsync(input, ct));
    }

    [HttpGet]
    [RequirePermission("products.manage")]
    public IActionResult CreateTax() => View(new InTaxCreateView { IsActive = true });

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateTax(InTaxCreateView model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await _svc.CreateTaxAsync(InputTaxCreate.From(model), ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = "Tax created.";
        return RedirectToAction(nameof(Taxes));
    }

    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditTax(int id, CancellationToken ct)
    {
        var item = await _svc.GetTaxAsync(new InputTaxSelect { IdTax = id }, ct);
        if (item is null) return NotFound();
        return View(item.ToUpdateView());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditTax(int id, InTaxUpdateView model, CancellationToken ct)
    {
        model.IdTax = id;
        if (!ModelState.IsValid) return View(model);
        var result = await _svc.UpdateTaxAsync(InputTaxUpdate.From(model), ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = "Tax updated.";
        return RedirectToAction(nameof(Taxes));
    }

    [HttpGet]
    public async Task<IActionResult> TaxLookup(CancellationToken ct)
        => Json(await _svc.GetTaxLookupAsync(ct));
}
