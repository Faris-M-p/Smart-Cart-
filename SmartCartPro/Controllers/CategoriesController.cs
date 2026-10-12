using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

public class CategoriesController : Controller
{
    private readonly ICategoryService _categories;

    public CategoriesController(ICategoryService categories)
    {
        _categories = categories;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _categories.ListAsync(cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var item = await _categories.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }
}
