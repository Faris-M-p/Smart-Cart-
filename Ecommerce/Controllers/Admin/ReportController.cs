using Ecommerce.Filters;
using Ecommerce.Interface.Admin;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.Admin.ReportModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Report")]
    public class ReportController : Controller
    {
        private readonly IReportInterface _reportInterface;

        public ReportController(IReportInterface reportInterface)
        {
            _reportInterface = reportInterface;
        }

        [Route("Sales")]
        [RequirePermission("Sales.View")]
        public IActionResult Sales()
        {
            return View("~/Views/Admin/Report/Sales.cshtml");
        }

        [HttpPost]
        [Route("GetSalesReport")]
        [RequirePermission("Sales.View")]
        public async Task<IActionResult> GetSalesReport([FromBody] SalesReportInput input)
        {
            try
            {
                var result = await _reportInterface.GetSalesReportAsync(input ?? new SalesReportInput());
                return Ok(new ApiResponse<SalesReportSummary> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [Route("Order")]
        [RequirePermission("Orders.View")]
        public IActionResult Order()
        {
            return View("~/Views/Admin/Report/Order.cshtml");
        }

        [HttpPost]
        [Route("GetOrderReport")]
        [RequirePermission("Orders.View")]
        public async Task<IActionResult> GetOrderReport([FromBody] OrderReportInput input)
        {
            try
            {
                var result = await _reportInterface.GetOrderReportAsync(input ?? new OrderReportInput());
                return Ok(new ApiResponse<TableOutput<OrderReportItem>> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [Route("Product")]
        [RequirePermission("Products.View")]
        public IActionResult Product()
        {
            return View("~/Views/Admin/Report/Product.cshtml");
        }

        [HttpPost]
        [Route("GetProductReport")]
        [RequirePermission("Products.View")]
        public async Task<IActionResult> GetProductReport([FromBody] ProductReportInput input)
        {
            try
            {
                var result = await _reportInterface.GetProductReportAsync(input ?? new ProductReportInput());
                return Ok(new ApiResponse<TableOutput<ProductReportItem>> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [Route("Customer")]
        [RequirePermission("Employees.View")]
        public IActionResult Customer()
        {
            return View("~/Views/Admin/Report/Customer.cshtml");
        }

        [HttpPost]
        [Route("GetCustomerReport")]
        [RequirePermission("Employees.View")]
        public async Task<IActionResult> GetCustomerReport([FromBody] CustomerReportInput input)
        {
            try
            {
                var result = await _reportInterface.GetCustomerReportAsync(input ?? new CustomerReportInput());
                return Ok(new ApiResponse<TableOutput<CustomerReportItem>> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }
    }
}
