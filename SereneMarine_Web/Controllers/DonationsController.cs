using Microsoft.AspNetCore.Mvc;

namespace SereneMarine_Web.Controllers
{
    public class DonationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
