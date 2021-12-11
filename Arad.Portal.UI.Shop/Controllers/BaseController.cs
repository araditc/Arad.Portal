using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class BaseController : Controller
    {
        public readonly string domainName;
        private readonly IHttpContextAccessor _accessor;
    
        public BaseController(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
            domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
        }
    }
}
