using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

public class OrdersController : Controller
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders)
    {
        _orders = orders;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _orders.ListAsync(cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var item = await _orders.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }
}
