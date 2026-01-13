using Ecommerce.Interface;
using Ecommerce.Repository;
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
            return View(); // No need for .cshtml extension
        }


        [HttpPost("Shop/GetProducts")]

        public async Task<IActionResult> GetProductList([FromBody] InputProduct input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest("Model validation error");
                }

                // Get the connection string or database name if required (same as in the Lead example)
            



                // Prepare the parameter object
                var outputData = await _shopInterface.GetProductListAsync( new InputProduct
                {
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    SearchName = input.SearchName,
                    SortColumn = input.SortColumn,
                    SortMode = input.SortMode,
                    CategoryIds = input.CategoryIds,
                    SubCategoryIds = input.SubCategoryIds,
                    BrandIds = input.BrandIds,
                    Ratings = input.Ratings,
                    Gender = input.Gender,
                    PriceFrom = input.PriceFrom,
                    PriceTo = input.PriceTo,
                    Status = input.Status
                });

                return Ok(outputData);
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, "Internal server error");
            }
        }

    }

}
