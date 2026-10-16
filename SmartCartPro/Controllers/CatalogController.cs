using Microsoft.AspNetCore.Mvc;

namespace SmartCartPro.Controllers;

/// <summary>Catalog: categories, brands, products, variants/SKU.</summary>
public class CatalogController : Controller
{
    public IActionResult Categories() => View();

    public IActionResult Brands() => View();

    public IActionResult Products() => View();

    public IActionResult Variants() => View();
}
