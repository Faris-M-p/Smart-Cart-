using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Common dropdown fill API (CRM-style).
/// Input: <see cref="FilterOptions"/>. Output: <see cref="FilterDataResponse"/> with typed lists.
/// </summary>
[ApiController]
[Route("[controller]")]
public class FilterController : ControllerBase
{
    private readonly IFilterService _filterService;

    public FilterController(IFilterService filterService)
    {
        _filterService = filterService;
    }

    /// <summary>
    /// GET /Filter/GetFilterData?mode=1
    /// GET /Filter/GetFilterData?mode=2
    /// GET /Filter/GetFilterData?mode=3&amp;criteria=12
    /// </summary>
    [HttpGet("GetFilterData")]
    public async Task<ActionResult<FilterDataResponse>> GetFilterData(
        [FromQuery] FilterOptions filter,
        CancellationToken cancellationToken)
    {
        if (filter.Mode <= 0)
        {
            return BadRequest(new { message = "Mode is required. Use 1=Products, 2=Categories, 3=SubCategories." });
        }

        try
        {
            var data = await _filterService.GetFilterDataAsync(filter, cancellationToken);
            return Ok(data);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
