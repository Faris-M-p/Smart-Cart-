using Ecommerce.Interface.Admin;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.Admin.InventoryModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Inventory")]
    public class InventoryController : Controller
    {
        private readonly IInventoryInterface _inventoryInterface;

        public InventoryController(IInventoryInterface inventoryInterface)
        {
            _inventoryInterface = inventoryInterface;
        }

        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Inventory/Index.cshtml");
        }

        [HttpPost]
        [Route("GetStockList")]
        public async Task<IActionResult> GetStockList([FromBody] StockListInputVIEW view)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new { message = "Validation failed.", errors });
                }

                var input = new StockListInput
                {
                    SearchText = view.SearchText,
                    LowStockOnly = view.LowStockOnly,
                    PageIndex = view.PageIndex,
                    PageSize = view.PageSize
                };

                var result = await _inventoryInterface.GetStockListAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("AdjustStock")]
        public async Task<IActionResult> AdjustStock([FromBody] AdjustStockInputVIEW view)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { message = "Validation failed.", errors });
                }

                var input = new AdjustStockInput
                {
                    FK_ProductVariant = view.FK_ProductVariant,
                    AdjustBy = view.AdjustBy,
                    Reason = view.Reason,
                    EnterBy = view.EnterBy
                };

                var result = await _inventoryInterface.AdjustStockAsync(input);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
    }
}

