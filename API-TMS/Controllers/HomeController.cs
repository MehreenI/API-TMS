using Microsoft.AspNetCore.Mvc;

namespace API_TMS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
