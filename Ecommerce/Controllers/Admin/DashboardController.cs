using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers.Admin
{
    [Route("Admin/Dashboard")]
    public class DashboardController : Controller
    {
        [HttpGet("/admin/dashboard")]
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Dashboard/Index.cshtml");
        }
    }
}
