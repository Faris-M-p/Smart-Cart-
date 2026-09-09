using Ecommerce.Filters;
using Ecommerce.Interface.Admin;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.Admin.SalesReturnModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/SalesReturn")]
    public class SalesReturnController : Controller
    {
        private readonly ISalesReturnInterface _returns;

        public SalesReturnController(ISalesReturnInterface returns)
        {
            _returns = returns;
        }

        [Route("")]
        [Route("Index")]
        [RequirePermission("SalesReturns.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/SalesReturn/Index.cshtml");
        }

        [HttpPost]
        [Route("GetSalesReturnList")]
        [RequirePermission("SalesReturns.View")]
        public async Task<IActionResult> GetSalesReturnList([FromBody] SalesReturnListInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed." });
            }

            var result = await _returns.GetSalesReturnListAsync(new SalesReturnListInput
            {
                SearchText = viewInput.SearchText,
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
        [RequirePermission("SalesReturns.View")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _returns.GetSalesReturnByIdAsync(id);
            if (result.ReturnHeader.ID_SalesReturn <= 0)
            {
                return NotFound(new { message = "Sales return not found." });
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("GetSales")]
        [RequirePermission("SalesReturns.View")]
        public async Task<IActionResult> GetSales()
        {
            return Ok(await _returns.GetSaleLookupsAsync());
        }

        [HttpGet]
        [Route("GetReturnableLines/{saleId:int}")]
        [RequirePermission("SalesReturns.View")]
        public async Task<IActionResult> GetReturnableLines(int saleId)
        {
            return Ok(await _returns.GetReturnableLinesAsync(saleId));
        }

        [HttpPost]
        [Route("Create")]
        [RequirePermission("SalesReturns.Create")]
        public async Task<IActionResult> Create([FromBody] SalesReturnUpdateInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed." });
            }

            if (viewInput.ReturnDetails == null || viewInput.ReturnDetails.Count == 0)
            {
                return BadRequest(new { message = "Enter at least one return quantity." });
            }

            var result = await _returns.CreateSalesReturnAsync(new SalesReturnUpdateInput
            {
                FK_Sale = viewInput.FK_Sale,
                ReturnDate = viewInput.ReturnDate,
                Reason = viewInput.Reason,
                Notes = viewInput.Notes,
                EnterBy = 1,
                ReturnDetails = viewInput.ReturnDetails
            });
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [RequirePermission("SalesReturns.Delete")]
        public async Task<IActionResult> Delete([FromBody] SalesReturnDeleteInputVIEW viewInput)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed." });
            }

            var result = await _returns.DeleteSalesReturnAsync(new SalesReturnDeleteInput
            {
                ID_SalesReturn = viewInput.ID_SalesReturn,
                CancelledReason = viewInput.CancelledReason,
                EnterBy = 1
            });
            return Ok(result);
        }
    }
}
