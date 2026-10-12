using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _products;

    public ProductsController(IProductService products)
    {
        _products = products;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _products.ListAsync(cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var item = await _products.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }
}
