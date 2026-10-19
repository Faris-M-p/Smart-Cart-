using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
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
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.CreateCategoryAsync(InputCategoryCreate.From(model), ct);
        if (!result.Ok)
        {
            throw new UpdateFailedException(
                "Unable to create category.",
                result.ResponseMsg ?? "Unable to create category.");
        }

        return CommonHelpers.SuccessResponse(
            "Category created successfully.",
            new { id = result.IdCategory });
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
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.UpdateCategoryAsync(InputCategoryUpdate.From(model), ct);
        if (!result.Ok)
        {
            throw new UpdateFailedException(
                "Unable to update category.",
                result.ResponseMsg ?? "Unable to update category.");
        }

        return CommonHelpers.SuccessResponse(
            "Category updated successfully.",
            new { id = result.IdCategory });
    }

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
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.CreateSubCategoryAsync(InputSubCategoryCreate.From(model), ct);
        if (!result.Ok)
        {
            throw new UpdateFailedException(
                "Unable to create sub-category.",
                result.ResponseMsg ?? "Unable to create sub-category.");
        }

        return CommonHelpers.SuccessResponse(
            "Sub-category created successfully.",
            new { id = result.IdSubCategory });
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
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.UpdateSubCategoryAsync(InputSubCategoryUpdate.From(model), ct);
        if (!result.Ok)
        {
            throw new UpdateFailedException(
                "Unable to update sub-category.",
                result.ResponseMsg ?? "Unable to update sub-category.");
        }

        return CommonHelpers.SuccessResponse(
            "Sub-category updated successfully.",
            new { id = result.IdSubCategory });
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
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.CreateBrandAsync(InputBrandCreate.From(model), ct);
        if (!result.Ok)
        {
            throw new UpdateFailedException(
                "Unable to create brand.",
                result.ResponseMsg ?? "Unable to create brand.");
        }

        return CommonHelpers.SuccessResponse(
            "Brand created successfully.",
            new { id = result.IdBrand });
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
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.UpdateBrandAsync(InputBrandUpdate.From(model), ct);
        if (!result.Ok)
        {
            throw new UpdateFailedException(
                "Unable to update brand.",
                result.ResponseMsg ?? "Unable to update brand.");
        }

        return CommonHelpers.SuccessResponse(
            "Brand updated successfully.",
            new { id = result.IdBrand });
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
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.CreateUnitAsync(InputUnitCreate.From(model), ct);
        if (!result.Ok)
        {
            throw new UpdateFailedException(
                "Unable to create unit.",
                result.ResponseMsg ?? "Unable to create unit.");
        }

        return CommonHelpers.SuccessResponse(
            "Unit created successfully.",
            new { id = result.IdUnit });
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
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.UpdateUnitAsync(InputUnitUpdate.From(model), ct);
        if (!result.Ok)
        {
            throw new UpdateFailedException(
                "Unable to update unit.",
                result.ResponseMsg ?? "Unable to update unit.");
        }

        return CommonHelpers.SuccessResponse(
            "Unit updated successfully.",
            new { id = result.IdUnit });
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
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.CreateTaxAsync(InputTaxCreate.From(model), ct);
        if (!result.Ok)
        {
            throw new UpdateFailedException(
                "Unable to create tax.",
                result.ResponseMsg ?? "Unable to create tax.");
        }

        return CommonHelpers.SuccessResponse(
            "Tax created successfully.",
            new { id = result.IdTax });
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
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.UpdateTaxAsync(InputTaxUpdate.From(model), ct);
        if (!result.Ok)
        {
            throw new UpdateFailedException(
                "Unable to update tax.",
                result.ResponseMsg ?? "Unable to update tax.");
        }

        return CommonHelpers.SuccessResponse(
            "Tax updated successfully.",
            new { id = result.IdTax });
    }

    [HttpGet]
    public async Task<IActionResult> TaxLookup(CancellationToken ct)
        => Json(await _svc.GetTaxLookupAsync(ct));
}
