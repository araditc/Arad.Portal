using Arad.Portal.UI.Shop.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Contracts.General.Domain;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class CaptchaController : BaseController
    {
        
        public CaptchaController(IHttpContextAccessor accessor, IDomainRepository domainRepository):base(accessor, domainRepository)
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
