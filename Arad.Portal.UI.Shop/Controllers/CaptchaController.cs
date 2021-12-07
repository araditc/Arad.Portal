using Arad.Portal.UI.Shop.Helpers;

using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class CaptchaController : Controller
    {
        [HttpGet]
        public IActionResult CaptchaImage()
        {
            FileContentResult captcha = HttpContext.Session.GenerateCaptchaImage(2);

            return captcha;
        }
    }
}
