using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Shared dropdown fill used by catalog forms (products, categories, sub-categories).
/// Staff with products.view call this from create/edit screens.
/// </summary>
[Authorize]
[ModuleAccess("PRODUCTS")]
public class FilterController : Controller
{
    private readonly IFilterService _filterService;

    public FilterController(IFilterService filterService)
    {
        _filterService = filterService;
    }

    /// <summary>
    /// Returns typed lookup lists for the requested mode (1 products, 2 categories, 3 sub-categories).
    /// Used by catalog forms that cascade dropdowns without a dedicated lookup action.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.view")]
    public async Task<IActionResult> GetFilterData([FromQuery] FilterOptions filter, CancellationToken cancellationToken)
    {
        if (filter.Mode <= 0)
        {
            throw new Exceptions.ValidationException(
                "Mode is required. Use 1=Products, 2=Categories, 3=SubCategories.",
                "Filter GetFilterData called with Mode <= 0.");
        }

        var data = await _filterService.GetFilterDataAsync(filter, cancellationToken);
        return CommonHelpers.SuccessResponse("OK", data);
    }
}
