using Ecommerce.Interface;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.ProductModel;

namespace Ecommerce.Controllers
{
    public class ShopController : Controller
    {
        private readonly ShopInterface _shopInterface;

        public ShopController(ShopInterface shopInterface)
        {
            _shopInterface = shopInterface;
        }

        public IActionResult Index()
        {
            ViewBag.Title = "Shop";
            return View();
        }

        [HttpGet("Shop/Details/{slug?}")]
        public IActionResult Details(string? slug)
        {
            ViewBag.Title = "Product details";
            ViewBag.Slug = slug;
            return View();
        }

        [HttpPost("Shop/GetProducts")]
        public async Task<IActionResult> GetProductList([FromBody] InputProduct input)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Model validation error");
            }

            var outputData = await _shopInterface.GetProductListAsync(input ?? new InputProduct());
            return Ok(outputData);
        }

        [HttpGet("Shop/GetFilters")]
        public async Task<IActionResult> GetFilters()
        {
            var lookups = await _shopInterface.GetFilterLookupsAsync();
            return Ok(lookups);
        }
    }
}
