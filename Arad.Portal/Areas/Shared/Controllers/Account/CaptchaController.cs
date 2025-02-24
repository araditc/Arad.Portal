using Microsoft.AspNetCore.Mvc;
using Arad.Portal.Helpers.Shared;

namespace Arad.Portal.Areas.Shared.Controllers.Account;

public class CaptchaController : Controller
{
    [HttpGet]
    [Area("Shared")]
    public IActionResult CaptchaImage()
    {
        FileContentResult captcha = HttpContext.Session.GenerateCaptchaImage(1);
        return captcha;
    }
}