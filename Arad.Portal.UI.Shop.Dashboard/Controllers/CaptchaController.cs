using Microsoft.AspNetCore.Mvc;
using Arad.Portal.UI.Shop.Dashboard.Helpers;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{

  
    public class CaptchaController : Controller
    {
        [HttpGet]
        public IActionResult CaptchaImage()
        {
            var captcha = HttpContext.Session.GenerateCaptchaImage(1);
            return captcha;
        }
    }
}