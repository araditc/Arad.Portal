using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.Controllers.Base;

public class ExceptionHandlerController : Controller
{
    public IActionResult PageNotFound()
    {
        return View("NotFound");
    }
}