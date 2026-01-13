using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers.Admin
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
