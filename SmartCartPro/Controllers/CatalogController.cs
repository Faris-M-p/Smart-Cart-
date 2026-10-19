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
    private readonly IUserService _userSvc;

    public CatalogController(ICatalogService svc, IUserService userSvc)
    {
        _svc = svc;
        _userSvc = userSvc;
    }

    // ═══════════════════════════════════════════════════════════════
    // CATEGORY  (views live in ~/Views/Category/)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Lists product categories that group SKUs for merchandising and reports.
    /// Staff with products.view use this from Products catalog admin.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Categories([FromQuery] InputCategoryList input, CancellationToken ct)
    {
        input.Page = Math.Max(1, input.Page);
        var model = await _svc.ListCategoriesAsync(input, ct);
        ViewBag.CanManage = await CanManageProductsAsync(ct);
        ViewBag.ListUrl = Url.Action(nameof(Categories))!;
        return View("~/Views/Category/Categories.cshtml", model);
    }

    /// <summary>
    /// Shows the form to add a new merchandising category for the store catalog.
    /// Managers with products.manage open this before assigning products to categories.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public IActionResult CreateCategory(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View("~/Views/Category/CreateCategory.cshtml", new InCategoryCreateView { IsActive = true });
    }

    /// <summary>
    /// Saves a new category used on product forms, POS filters, and online listings.
    /// Managers submit the create category form after entering code, name, and display order.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateCategory(InCategoryCreateView model, string? returnUrl, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputCategoryCreate
        {
            CategoryCode = model.CategoryCode,
            CategoryName = model.CategoryName,
            Description = model.Description,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        };
        var result = await _svc.CreateCategoryAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A category with this name already exists.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to create category.",
                result.ResponseMsg ?? "Unable to create category.");
        }

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Categories))!;
        return CommonHelpers.SuccessResponse(
            "Category created successfully.",
            new { id = result.IdCategory, redirectUrl = listUrl });
    }

    /// <summary>
    /// Loads one category for editing name, sort order, or active flag.
    /// Managers with products.manage navigate here from the category list.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditCategory(int id, string? returnUrl, CancellationToken ct)
    {
        var item = await _svc.GetCategoryAsync(new InputCategorySelect { IdCategory = id }, ct);
        if (item is null) return NotFound();
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View("~/Views/Category/EditCategory.cshtml", item.ToUpdateView());
    }

    /// <summary>
    /// Persists category changes reflected on products and checkout item grouping.
    /// Managers submit the edit category form after updating catalog master data.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditCategory(int id, InCategoryUpdateView model, string? returnUrl, CancellationToken ct)
    {
        model.IdCategory = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputCategoryUpdate
        {
            IdCategory = model.IdCategory,
            CategoryCode = model.CategoryCode,
            CategoryName = model.CategoryName,
            Description = model.Description,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        };
        var result = await _svc.UpdateCategoryAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 404)
            {
                throw new ResourceNotFoundException(
                    result.ResponseMsg ?? "Category not found.",
                    result.ResponseMsg);
            }

            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A category with this name already exists.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to update category.",
                result.ResponseMsg ?? "Unable to update category.");
        }

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Categories))!;
        return CommonHelpers.SuccessResponse(
            "Category updated successfully.",
            new { id = result.IdCategory, redirectUrl = listUrl });
    }

    /// <summary>
    /// Activates or deactivates a category from the list without opening the edit form.
    /// Managers with products.manage use this after confirming the status change.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> SetCategoryActive(int id, bool isActive, CancellationToken ct)
    {
        var item = await _svc.GetCategoryAsync(new InputCategorySelect { IdCategory = id }, ct);
        if (item is null)
        {
            throw new ResourceNotFoundException("Category not found.", $"IdCategory={id}");
        }

        var input = new InputCategoryUpdate
        {
            IdCategory = item.IdCategory,
            CategoryCode = item.CategoryCode,
            CategoryName = item.CategoryName,
            Description = item.Description,
            DisplayOrder = item.DisplayOrder,
            IsActive = isActive
        };
        var result = await _svc.UpdateCategoryAsync(input, ct);
        if (!result.Ok)
        {
            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to update category status.",
                result.ResponseMsg ?? "Unable to update category status.");
        }

        return CommonHelpers.SuccessResponse(
            isActive ? "Category activated." : "Category deactivated.",
            new { id = result.IdCategory, isActive });
    }

    /// <summary>
    /// Returns active categories as JSON for product and filter dropdowns.
    /// Product create/edit screens and POS catalog filters call this via AJAX.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CategoryLookup(CancellationToken ct)
        => Json(await _svc.GetCategoryLookupAsync(ct));

    /// <summary>
    /// Returns subcategories for a selected parent category as JSON.
    /// Used when cascading category picks on product forms and inventory filters.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SubCategoryLookup(int fkCategory, CancellationToken ct)
        => Json(await _svc.GetSubCategoryLookupAsync(fkCategory, ct));

    // ═══════════════════════════════════════════════════════════════
    // SUB-CATEGORY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Lists subcategories under parent categories for finer product organization.
    /// Staff with products.view browse this from catalog admin when maintaining hierarchy.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> SubCategories([FromQuery] InputSubCategoryList input, CancellationToken ct)
    {
        var model = await _svc.ListSubCategoriesAsync(input, ct);
        ViewBag.Categories = await _svc.GetCategoryOptionsAsync(ct);
        return View("~/Views/SubCategory/SubCategories.cshtml", model);
    }

    /// <summary>
    /// Shows the form to add a subcategory linked to a parent category.
    /// Managers with products.manage use it when expanding aisle or menu groupings.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateSubCategory(int? fkCategory, string? returnUrl, CancellationToken ct)
    {
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        ViewBag.Categories = await _svc.GetCategoryOptionsAsync(ct);
        return View(
            "~/Views/SubCategory/CreateSubCategory.cshtml",
            new InSubCategoryCreateView { FkCategory = fkCategory ?? 0, IsActive = true });
    }

    /// <summary>
    /// Saves a new subcategory for product assignment and POS or online filtering.
    /// Managers submit after choosing parent category and subcategory details.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateSubCategory(InSubCategoryCreateView model, string? returnUrl, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputSubCategoryCreate
        {
            FkCategory = model.FkCategory,
            SubCategoryName = model.SubCategoryName,
            Description = model.Description,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        };
        var result = await _svc.CreateSubCategoryAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 400)
            {
                var message = string.IsNullOrWhiteSpace(result.ResponseMsg)
                    ? "Please select an active Category."
                    : result.ResponseMsg;
                throw new ValidationException(
                    message,
                    message,
                    new Dictionary<string, string[]>
                    {
                        [nameof(InSubCategoryCreateView.FkCategory)] = new[] { message }
                    });
            }

            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A sub-category with this name already exists in the selected category.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to create sub-category.",
                result.ResponseMsg ?? "Unable to create sub-category.");
        }

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(SubCategories))!;
        return CommonHelpers.SuccessResponse(
            "Sub-category created successfully.",
            new { id = result.IdSubCategory, redirectUrl = listUrl });
    }

    /// <summary>
    /// Loads one subcategory for editing parent link, name, or active status.
    /// Managers open this from the subcategory list in catalog admin.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditSubCategory(int id, string? returnUrl, CancellationToken ct)
    {
        var item = await _svc.GetSubCategoryAsync(new InputSubCategorySelect { IdSubCategory = id }, ct);
        if (item is null) return NotFound();
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        ViewBag.Categories = await _svc.GetCategoryOptionsAsync(ct);
        return View("~/Views/SubCategory/EditSubCategory.cshtml", item.ToUpdateView());
    }

    /// <summary>
    /// Persists subcategory changes used on product records and sales reports.
    /// Managers submit the edit subcategory form after updating master data.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditSubCategory(int id, InSubCategoryUpdateView model, string? returnUrl, CancellationToken ct)
    {
        model.IdSubCategory = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputSubCategoryUpdate
        {
            IdSubCategory = model.IdSubCategory,
            FkCategory = model.FkCategory,
            SubCategoryName = model.SubCategoryName,
            Description = model.Description,
            DisplayOrder = model.DisplayOrder,
            IsActive = model.IsActive
        };
        var result = await _svc.UpdateSubCategoryAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 404)
            {
                throw new ResourceNotFoundException(
                    result.ResponseMsg ?? "Sub-category not found.",
                    result.ResponseMsg);
            }

            if (result.StatusCode == 400)
            {
                var message = string.IsNullOrWhiteSpace(result.ResponseMsg)
                    ? "Please select an active Category."
                    : result.ResponseMsg;
                throw new ValidationException(
                    message,
                    message,
                    new Dictionary<string, string[]>
                    {
                        [nameof(InSubCategoryUpdateView.FkCategory)] = new[] { message }
                    });
            }

            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A sub-category with this name already exists in the selected category.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to update sub-category.",
                result.ResponseMsg ?? "Unable to update sub-category.");
        }

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(SubCategories))!;
        return CommonHelpers.SuccessResponse(
            "Sub-category updated successfully.",
            new { id = result.IdSubCategory, redirectUrl = listUrl });
    }

    /// <summary>
    /// Activates or deactivates a subcategory from the list without opening the edit form.
    /// Managers with products.manage use this after confirming the status change.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> SetSubCategoryActive(int id, bool isActive, CancellationToken ct)
    {
        var item = await _svc.GetSubCategoryAsync(new InputSubCategorySelect { IdSubCategory = id }, ct);
        if (item is null)
        {
            throw new ResourceNotFoundException("Sub-category not found.", $"IdSubCategory={id}");
        }

        var input = new InputSubCategoryUpdate
        {
            IdSubCategory = item.IdSubCategory,
            FkCategory = item.FkCategory,
            SubCategoryName = item.SubCategoryName,
            Description = item.Description,
            DisplayOrder = item.DisplayOrder,
            IsActive = isActive
        };
        var result = await _svc.UpdateSubCategoryAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 404)
            {
                throw new ResourceNotFoundException(
                    result.ResponseMsg ?? "Sub-category not found.",
                    result.ResponseMsg);
            }

            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A sub-category with this name already exists in the selected category.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to update sub-category status.",
                result.ResponseMsg ?? "Unable to update sub-category status.");
        }

        return CommonHelpers.SuccessResponse(
            isActive ? "Sub-category activated." : "Sub-category deactivated.",
            new { id = result.IdSubCategory, isActive });
    }

    // ═══════════════════════════════════════════════════════════════
    // BRAND
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Lists product brands for supplier alignment and shelf labeling.
    /// Staff with products.view maintain brand masters from catalog admin.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Brands([FromQuery] InputBrandList input, CancellationToken ct)
    {
        var model = await _svc.ListBrandsAsync(input, ct);
        return View("~/Views/Brand/Brands.cshtml", model);
    }

    /// <summary>
    /// Shows the form to register a brand name for new or existing products.
    /// Managers with products.manage add brands before linking items at receiving.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public IActionResult CreateBrand(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View("~/Views/Brand/CreateBrand.cshtml", new InBrandCreateView { IsActive = true });
    }

    /// <summary>
    /// Saves a new brand available on product create and purchase line items.
    /// Managers submit the create brand form from catalog maintenance.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateBrand(InBrandCreateView model, string? returnUrl, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputBrandCreate
        {
            BrandName = model.BrandName,
            Description = model.Description,
            IsActive = model.IsActive
        };
        var result = await _svc.CreateBrandAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A brand with this name already exists.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to create brand.",
                result.ResponseMsg ?? "Unable to create brand.");
        }

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Brands))!;
        return CommonHelpers.SuccessResponse(
            "Brand created successfully.",
            new { id = result.IdBrand, redirectUrl = listUrl });
    }

    /// <summary>
    /// Loads one brand record for editing name, description, or active flag.
    /// Managers navigate here from the brand list when correcting catalog data.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditBrand(int id, string? returnUrl, CancellationToken ct)
    {
        var item = await _svc.GetBrandAsync(new InputBrandSelect { IdBrand = id }, ct);
        if (item is null) return NotFound();
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View("~/Views/Brand/EditBrand.cshtml", item.ToUpdateView());
    }

    /// <summary>
    /// Persists brand updates shown on product labels and inventory reports.
    /// Managers submit the edit brand form after revising master details.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditBrand(int id, InBrandUpdateView model, string? returnUrl, CancellationToken ct)
    {
        model.IdBrand = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputBrandUpdate
        {
            IdBrand = model.IdBrand,
            BrandName = model.BrandName,
            Description = model.Description,
            IsActive = model.IsActive
        };
        var result = await _svc.UpdateBrandAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 404)
            {
                throw new ResourceNotFoundException(
                    result.ResponseMsg ?? "Brand not found.",
                    result.ResponseMsg);
            }

            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A brand with this name already exists.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to update brand.",
                result.ResponseMsg ?? "Unable to update brand.");
        }

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Brands))!;
        return CommonHelpers.SuccessResponse(
            "Brand updated successfully.",
            new { id = result.IdBrand, redirectUrl = listUrl });
    }

    /// <summary>
    /// Activates or deactivates a brand from the list without opening the edit form.
    /// Managers with products.manage use this after confirming the status change.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> SetBrandActive(int id, bool isActive, CancellationToken ct)
    {
        var item = await _svc.GetBrandAsync(new InputBrandSelect { IdBrand = id }, ct);
        if (item is null)
        {
            throw new ResourceNotFoundException("Brand not found.", $"IdBrand={id}");
        }

        var input = new InputBrandUpdate
        {
            IdBrand = item.IdBrand,
            BrandName = item.BrandName,
            Description = item.Description,
            IsActive = isActive
        };
        var result = await _svc.UpdateBrandAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 404)
            {
                throw new ResourceNotFoundException(
                    result.ResponseMsg ?? "Brand not found.",
                    result.ResponseMsg);
            }

            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A brand with this name already exists.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to update brand status.",
                result.ResponseMsg ?? "Unable to update brand status.");
        }

        return CommonHelpers.SuccessResponse(
            isActive ? "Brand activated." : "Brand deactivated.",
            new { id = result.IdBrand, isActive });
    }

    /// <summary>
    /// Returns active brands as JSON for product form brand pickers.
    /// Product create and edit screens request this when loading dropdown options.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BrandLookup(CancellationToken ct)
        => Json(await _svc.GetBrandLookupAsync(ct));

    // ═══════════════════════════════════════════════════════════════
    // UNIT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Lists units of measure for stocking, pricing, and receipt quantities.
    /// Staff with products.view review units from catalog admin before POS go-live.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Units([FromQuery] InputUnitList input, CancellationToken ct)
    {
        var model = await _svc.ListUnitsAsync(input, ct);
        return View("~/Views/Unit/Units.cshtml", model);
    }

    /// <summary>
    /// Shows the form to define a sell or stock unit such as each, kg, or pack.
    /// Managers with products.manage add units before assigning them to SKUs.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public IActionResult CreateUnit(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View("~/Views/Unit/CreateUnit.cshtml", new InUnitCreateView { IsActive = true });
    }

    /// <summary>
    /// Saves a new unit used on product cost, quantity, and barcode sale lines.
    /// Managers submit the create unit form during catalog setup.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateUnit(InUnitCreateView model, string? returnUrl, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputUnitCreate
        {
            UnitCode = model.UnitCode,
            UnitName = model.UnitName,
            IsActive = model.IsActive
        };
        var result = await _svc.CreateUnitAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A unit with this code or name already exists.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to create unit.",
                result.ResponseMsg ?? "Unable to create unit.");
        }

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Units))!;
        return CommonHelpers.SuccessResponse(
            "Unit created successfully.",
            new { id = result.IdUnit, redirectUrl = listUrl });
    }

    /// <summary>
    /// Loads one unit for editing code, display name, or active status.
    /// Managers open this from the unit list when correcting measure labels.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditUnit(int id, string? returnUrl, CancellationToken ct)
    {
        var item = await _svc.GetUnitAsync(new InputUnitSelect { IdUnit = id }, ct);
        if (item is null) return NotFound();
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View("~/Views/Unit/EditUnit.cshtml", item.ToUpdateView());
    }

    /// <summary>
    /// Persists unit changes reflected on products and purchase order quantities.
    /// Managers submit the edit unit form after updating master data.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditUnit(int id, InUnitUpdateView model, string? returnUrl, CancellationToken ct)
    {
        model.IdUnit = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputUnitUpdate
        {
            IdUnit = model.IdUnit,
            UnitCode = model.UnitCode,
            UnitName = model.UnitName,
            IsActive = model.IsActive
        };
        var result = await _svc.UpdateUnitAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 404)
            {
                throw new ResourceNotFoundException(
                    result.ResponseMsg ?? "Unit not found.",
                    result.ResponseMsg);
            }

            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A unit with this code or name already exists.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to update unit.",
                result.ResponseMsg ?? "Unable to update unit.");
        }

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Units))!;
        return CommonHelpers.SuccessResponse(
            "Unit updated successfully.",
            new { id = result.IdUnit, redirectUrl = listUrl });
    }

    /// <summary>
    /// Activates or deactivates a unit from the list without opening the edit form.
    /// Managers with products.manage use this after confirming the status change.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> SetUnitActive(int id, bool isActive, CancellationToken ct)
    {
        var item = await _svc.GetUnitAsync(new InputUnitSelect { IdUnit = id }, ct);
        if (item is null)
        {
            throw new ResourceNotFoundException("Unit not found.", $"IdUnit={id}");
        }

        var input = new InputUnitUpdate
        {
            IdUnit = item.IdUnit,
            UnitCode = item.UnitCode,
            UnitName = item.UnitName,
            IsActive = isActive
        };
        var result = await _svc.UpdateUnitAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 404)
            {
                throw new ResourceNotFoundException(
                    result.ResponseMsg ?? "Unit not found.",
                    result.ResponseMsg);
            }

            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A unit with this code or name already exists.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to update unit status.",
                result.ResponseMsg ?? "Unable to update unit status.");
        }

        return CommonHelpers.SuccessResponse(
            isActive ? "Unit activated." : "Unit deactivated.",
            new { id = result.IdUnit, isActive });
    }

    /// <summary>
    /// Returns active units as JSON for product unit dropdowns.
    /// Product forms and receiving screens call this when loading measure options.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> UnitLookup(CancellationToken ct)
        => Json(await _svc.GetUnitLookupAsync(ct));

    // ═══════════════════════════════════════════════════════════════
    // TAX
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Lists tax profiles applied at POS checkout and on product pricing.
    /// Staff with products.view review tax masters from catalog admin.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> Taxes([FromQuery] InputTaxList input, CancellationToken ct)
    {
        var model = await _svc.ListTaxesAsync(input, ct);
        return View("~/Views/Tax/Taxes.cshtml", model);
    }

    /// <summary>
    /// Shows the form to define a tax rate and inclusive or exclusive behavior.
    /// Managers with products.manage configure taxes before assigning them to items.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public IActionResult CreateTax(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View("~/Views/Tax/CreateTax.cshtml", new InTaxCreateView { IsActive = true });
    }

    /// <summary>
    /// Saves a new tax profile used when ringing sales and printing receipts.
    /// Managers submit the create tax form during store fiscal setup.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> CreateTax(InTaxCreateView model, string? returnUrl, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputTaxCreate
        {
            TaxCode = model.TaxCode,
            TaxName = model.TaxName,
            TaxRate = model.TaxRate,
            IsInclusive = model.IsInclusive,
            IsActive = model.IsActive
        };
        var result = await _svc.CreateTaxAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A tax with this code or name already exists.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to create tax.",
                result.ResponseMsg ?? "Unable to create tax.");
        }

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Taxes))!;
        return CommonHelpers.SuccessResponse(
            "Tax created successfully.",
            new { id = result.IdTax, redirectUrl = listUrl });
    }

    /// <summary>
    /// Loads one tax record for editing rate, code, or inclusive pricing flag.
    /// Managers navigate here from the tax list when regulations or rates change.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditTax(int id, string? returnUrl, CancellationToken ct)
    {
        var item = await _svc.GetTaxAsync(new InputTaxSelect { IdTax = id }, ct);
        if (item is null) return NotFound();
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View("~/Views/Tax/EditTax.cshtml", item.ToUpdateView());
    }

    /// <summary>
    /// Persists tax updates applied to linked products and open register totals.
    /// Managers submit the edit tax form after revising compliance settings.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> EditTax(int id, InTaxUpdateView model, string? returnUrl, CancellationToken ct)
    {
        model.IdTax = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputTaxUpdate
        {
            IdTax = model.IdTax,
            TaxCode = model.TaxCode,
            TaxName = model.TaxName,
            TaxRate = model.TaxRate,
            IsInclusive = model.IsInclusive,
            IsActive = model.IsActive
        };
        var result = await _svc.UpdateTaxAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 404)
            {
                throw new ResourceNotFoundException(
                    result.ResponseMsg ?? "Tax not found.",
                    result.ResponseMsg);
            }

            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A tax with this code or name already exists.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to update tax.",
                result.ResponseMsg ?? "Unable to update tax.");
        }

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Taxes))!;
        return CommonHelpers.SuccessResponse(
            "Tax updated successfully.",
            new { id = result.IdTax, redirectUrl = listUrl });
    }

    /// <summary>
    /// Activates or deactivates a tax profile from the list without opening the edit form.
    /// Managers with products.manage use this after confirming the status change.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> SetTaxActive(int id, bool isActive, CancellationToken ct)
    {
        var item = await _svc.GetTaxAsync(new InputTaxSelect { IdTax = id }, ct);
        if (item is null)
        {
            throw new ResourceNotFoundException("Tax not found.", $"IdTax={id}");
        }

        var input = new InputTaxUpdate
        {
            IdTax = item.IdTax,
            TaxCode = item.TaxCode,
            TaxName = item.TaxName,
            TaxRate = item.TaxRate,
            IsInclusive = item.IsInclusive,
            IsActive = isActive
        };
        var result = await _svc.UpdateTaxAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 404)
            {
                throw new ResourceNotFoundException(
                    result.ResponseMsg ?? "Tax not found.",
                    result.ResponseMsg);
            }

            if (result.StatusCode == 409)
            {
                throw new ConflictException(
                    result.ResponseMsg ?? "A tax with this code or name already exists.",
                    result.ResponseMsg);
            }

            throw new UpdateFailedException(
                result.ResponseMsg ?? "Unable to update tax status.",
                result.ResponseMsg ?? "Unable to update tax status.");
        }

        return CommonHelpers.SuccessResponse(
            isActive ? "Tax activated." : "Tax deactivated.",
            new { id = result.IdTax, isActive });
    }

    /// <summary>
    /// Returns active tax profiles as JSON for product tax assignment dropdowns.
    /// Product create and edit screens request this when loading fiscal options.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> TaxLookup(CancellationToken ct)
        => Json(await _svc.GetTaxLookupAsync(ct));

    private async Task<bool> CanManageProductsAsync(CancellationToken ct)
    {
        if (AuthClaims.IsPlatformSuperAdminUser(User))
            return true;

        var userId = AuthClaims.GetUserId(User);
        if (userId is null or 0)
            return false;

        var permissions = await _userSvc.GetUserPermissionsAsync(userId.Value, ct);
        return permissions.Any(p =>
            string.Equals(p.PermissionCode, "products.manage", StringComparison.OrdinalIgnoreCase));
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
