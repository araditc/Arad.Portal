using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ExceptionHandlerController : Controller
    {
        public IActionResult PageNotFound()
        {
            return View("NotFound");
        }
    }
}
