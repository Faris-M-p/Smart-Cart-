using Ecommerce.Filters;
using Ecommerce.Interface.Admin;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.Admin.AdminOrderModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Order")]
    public class OrderController : Controller
    {
        private readonly IAdminOrderInterface _adminOrderInterface;

        public OrderController(IAdminOrderInterface adminOrderInterface)
        {
            _adminOrderInterface = adminOrderInterface;
        }

        [Route("")]
        [Route("Index")]
        [RequirePermission("Orders.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Order/Index.cshtml");
        }

        [Route("Details/{id:int}")]
        [RequirePermission("Orders.View")]
        public IActionResult Details(int id)
        {
            ViewBag.OrderId = id;
            return View("~/Views/Admin/Order/Details.cshtml");
        }

        [HttpPost]
        [Route("GetOrderList")]
        [RequirePermission("Orders.View")]
        public async Task<IActionResult> GetOrderList([FromBody] AdminOrderListInputVIEW viewInput)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new ApiResponse<TableOutput<AdminOrderListItem>>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = errors
                    });
                }

                var result = await _adminOrderInterface.GetOrderListAsync(new AdminOrderListInput
                {
                    SearchText = viewInput.SearchText,
                    OrderStatus = viewInput.OrderStatus,
                    FromDate = viewInput.FromDate,
                    ToDate = viewInput.ToDate,
                    PageIndex = viewInput.PageIndex,
                    PageSize = viewInput.PageSize
                });

                return Ok(new ApiResponse<TableOutput<AdminOrderListItem>>
                {
                    Success = true,
                    Message = "Orders loaded successfully",
                    Data = result
                });
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetById/{id:int}")]
        [RequirePermission("Orders.View")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var detail = await _adminOrderInterface.GetOrderByIdAsync(id);
                if (detail == null)
                {
                    return NotFound(new { message = "Order not found." });
                }

                return Ok(detail);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Confirm")]
        [RequirePermission("Orders.Edit")]
        public async Task<IActionResult> Confirm([FromBody] AdminOrderIdInputVIEW viewInput)
        {
            return await RunOrderActionAsync(viewInput, () => _adminOrderInterface.ConfirmOrderAsync(viewInput.OrderId));
        }

        [HttpPost]
        [Route("UpdateStatus")]
        [RequirePermission("Orders.Edit")]
        public async Task<IActionResult> UpdateStatus([FromBody] AdminOrderStatusInputVIEW viewInput)
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

                var result = await _adminOrderInterface.UpdateOrderStatusAsync(viewInput.OrderId, viewInput.OrderStatus);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        [HttpPost]
        [Route("Deliver")]
        [RequirePermission("Orders.Edit")]
        public async Task<IActionResult> Deliver([FromBody] AdminOrderIdInputVIEW viewInput)
        {
            return await RunOrderActionAsync(viewInput, () => _adminOrderInterface.DeliverOrderAsync(viewInput.OrderId));
        }

        [HttpPost]
        [Route("Cancel")]
        [RequirePermission("Orders.Cancel")]
        public async Task<IActionResult> Cancel([FromBody] AdminOrderCancelInputVIEW viewInput)
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

                var result = await _adminOrderInterface.CancelOrderAsync(viewInput.OrderId, viewInput.Reason);
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }

        private async Task<IActionResult> RunOrderActionAsync(AdminOrderIdInputVIEW viewInput, Func<Task<CommonResponse>> action)
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

                var result = await action();
                return Ok(result);
            }
            catch
            {
                throw;
            }
        }
    }
}
