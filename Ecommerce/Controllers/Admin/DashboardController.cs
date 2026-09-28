using Ecommerce.Filters;
using Ecommerce.Interface.Admin;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.ReportModel;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Dashboard")]
    public class DashboardController : Controller
    {
        private readonly IReportInterface _reportInterface;

        public DashboardController(IReportInterface reportInterface)
        {
            _reportInterface = reportInterface;
        }

        [HttpGet("/admin/dashboard")]
        [Route("")]
        [Route("Index")]
        [RequirePermission("Dashboard.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Dashboard/Index.cshtml");
        }

        [HttpGet]
        [Route("GetStats")]
        [RequirePermission("Dashboard.View")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _reportInterface.GetDashboardStatsAsync();
                return Ok(new ApiResponse<DashboardStats> { Success = true, Data = stats });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }
    }
}
