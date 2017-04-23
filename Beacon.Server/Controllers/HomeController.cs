using Microsoft.AspNetCore.Mvc;

namespace Beacon.Server.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}