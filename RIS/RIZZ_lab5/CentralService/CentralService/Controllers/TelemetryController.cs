using Microsoft.AspNetCore.Mvc;

namespace CentralService.Controllers
{
    public class TelemetryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
