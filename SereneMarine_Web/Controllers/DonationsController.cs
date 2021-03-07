using Microsoft.AspNetCore.Mvc;

namespace SereneMarine_Web.Controllers
{
    public class DonationsController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
