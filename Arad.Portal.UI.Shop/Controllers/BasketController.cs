using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class BasketController : BaseController
    {
        public BasketController(IHttpContextAccessor accessor):base(accessor)
        {
                
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
