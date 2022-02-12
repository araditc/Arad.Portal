using Arad.Portal.UI.Shop.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class CaptchaController : BaseController
    {
        public CaptchaController(IHttpContextAccessor accessor):base(accessor)
        {
                
        }
        [HttpGet]
        public IActionResult CaptchaImage()
        {
            FileContentResult captcha = HttpContext.Session.GenerateCaptchaImage(2);

            return captcha;
        }

       
    }
}
