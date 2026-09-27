using Ecommerce.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly HomeInterface _homeInterface;

        public HomeController(HomeInterface homeInterface)
        {
            _homeInterface = homeInterface;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _homeInterface.GetHomePageDataAsync();
            return View(model);
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
