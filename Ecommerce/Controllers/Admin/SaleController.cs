using Ecommerce.Filters;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.Admin.ProductModel;
using static Ecommerce.Models.Admin.SaleModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Sale")]
    public class SaleController : Controller
    {
        private readonly ISaleInterface _sales;
        private readonly IProductInterface _products;

        public SaleController(ISaleInterface sales, IProductInterface products)
        {
            _sales = sales;
            _products = products;
        }

        [Route("")]
        [Route("Index")]
        [RequirePermission("Sales.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Sale/Index.cshtml");
        }

        [HttpPost]
        [Route("GetSaleList")]
        [RequirePermission("Sales.View")]
        public async Task<IActionResult> GetSaleList([FromBody] SaleListInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed." });
            }

            var result = await _sales.GetSaleListAsync(new SaleListInput
            {
                SearchText = viewInput.SearchText,
                PaymentMethod = viewInput.PaymentMethod,
                FromDate = viewInput.FromDate,
                ToDate = viewInput.ToDate,
                PageIndex = viewInput.PageIndex,
                PageSize = viewInput.PageSize,
                SortColumn = viewInput.SortColumn,
                SortMode = viewInput.SortMode
            });
            return Ok(result);
        }

        [HttpGet]
        [Route("GetById/{id:int}")]
        [RequirePermission("Sales.View")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _sales.GetSaleByIdAsync(id);
            if (result.SaleHeader.ID_Sale <= 0)
            {
                return NotFound(new { message = "Sale not found." });
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("GetProducts")]
        [RequirePermission("Sales.View")]
        public async Task<IActionResult> GetProducts()
        {
            var result = await _products.GetProductListAsync(new ProductListInput
            {
                SearchText = string.Empty,
                FilterCategoryIDs = string.Empty,
                FilterSubCategoryIDs = string.Empty,
                FilterBrandIDs = string.Empty,
                PageIndex = 1,
                PageSize = 1000,
                SortColumn = (int)ProductSortColumn.Name,
                SortMode = "ASC"
            });
            var products = result.TableData?.Where(p => !p.Cancelled).ToList() ?? new List<Product>();
            return Ok(products);
        }

        [HttpGet]
        [Route("GetProductVariants/{productId:int}")]
        [RequirePermission("Sales.View")]
        public async Task<IActionResult> GetProductVariants(int productId)
        {
            var options = await _sales.GetSkuOptionsAsync(productId);
            return Ok(options);
        }

        [HttpGet]
        [Route("SearchSkus")]
        [RequirePermission("Sales.View")]
        public async Task<IActionResult> SearchSkus([FromQuery] string q = "")
        {
            var options = await _sales.SearchSkusAsync(q);
            return Ok(options);
        }

        [HttpGet]
        [Route("SearchCustomers")]
        [RequirePermission("Sales.View")]
        public async Task<IActionResult> SearchCustomers([FromQuery] string q = "")
        {
            var options = await _sales.SearchCustomersAsync(q);
            return Ok(options);
        }

        [HttpGet]
        [Route("Adjacent/{id:int}")]
        [RequirePermission("Sales.View")]
        public async Task<IActionResult> Adjacent(int id, [FromQuery] int dir = 1)
        {
            var result = await _sales.GetAdjacentSaleAsync(id, dir);
            if (result.Sale == null)
            {
                return NotFound(new { message = dir < 0 ? "No previous bill." : "No next bill." });
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("Create")]
        [RequirePermission("Sales.Create")]
        public async Task<IActionResult> Create([FromBody] SaleUpdateInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed." });
            }

            if (viewInput.SaleDetails == null || viewInput.SaleDetails.Count == 0)
            {
                return BadRequest(new { message = "At least one sale item is required." });
            }

            var result = await _sales.CreateSaleAsync(MapUpdate(viewInput));
            return Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [RequirePermission("Sales.Edit")]
        public async Task<IActionResult> Update([FromBody] SaleUpdateInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed." });
            }

            if (viewInput.SaleDetails == null || viewInput.SaleDetails.Count == 0)
            {
                return BadRequest(new { message = "At least one sale item is required." });
            }

            var result = await _sales.UpdateSaleAsync(MapUpdate(viewInput));
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [RequirePermission("Sales.Delete")]
        public async Task<IActionResult> Delete([FromBody] SaleDeleteInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed." });
            }

            var result = await _sales.DeleteSaleAsync(new SaleDeleteInput
            {
                ID_Sale = viewInput.ID_Sale,
                CancelledReason = viewInput.CancelledReason,
                EnterBy = 1
            });
            return Ok(result);
        }

        private static SaleUpdateInput MapUpdate(SaleUpdateInputVIEW viewInput) => new()
        {
            ID_Sale = viewInput.ID_Sale,
            SaleDate = viewInput.SaleDate,
            CustomerName = viewInput.CustomerName,
            CustomerPhone = viewInput.CustomerPhone,
            PaymentMethod = viewInput.PaymentMethod,
            InvoiceNumber = viewInput.InvoiceNumber,
            Notes = viewInput.Notes,
            EnterBy = 1,
            SaleDetails = viewInput.SaleDetails
        };
    }
}
