using Ecommerce.Filters;
using Ecommerce.Interface.Admin;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.Admin.DashboardModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Dashboard")]
    public class DashboardController : Controller
    {
        private readonly IDashboardInterface _dashboardInterface;

        public DashboardController(IDashboardInterface dashboardInterface)
        {
            _dashboardInterface = dashboardInterface;
        }

        [HttpGet("/admin/dashboard")]
        [Route("")]
        [Route("Index")]
        [RequirePermission("Dashboard.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Dashboard/Index.cshtml");
        }

        [HttpGet("GetSummary")]
        [RequirePermission("Dashboard.View")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            try
            {
                var data = await _dashboardInterface.GetSummaryAsync(cancellationToken);
                return Ok(new ApiResponse<DashboardSummary>
                {
                    Success = true,
                    Message = "Dashboard summary loaded.",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<DashboardSummary>
                {
                    Success = false,
                    Message = "Failed to load dashboard summary.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
