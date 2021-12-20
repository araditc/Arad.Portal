using Microsoft.AspNetCore.Mvc;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{


    [Authorize(Policy = "Role")]
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