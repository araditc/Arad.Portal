using Arad.Portal.UI.Shop.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class CaptchaController : BaseController
    {
        
        public CaptchaController(IHttpContextAccessor accessor, IWebHostEnvironment env):base(accessor, env)
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
