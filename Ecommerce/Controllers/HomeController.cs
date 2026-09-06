using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewBag.Title = "About Us";
            return View();
        }

        public IActionResult Blog()
        {
            ViewBag.Title = "Blog";
            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Title = "Contact";
            return View();
        }
    }
}
