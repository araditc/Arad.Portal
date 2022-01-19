using Arad.Portal.UI.Shop.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class PostController : BaseController
    {
        private readonly HttpClientHelper _httpClientHelper;
        private readonly IHttpContextAccessor _accessor;
        public PostController(HttpClientHelper httpClientHelper, IHttpContextAccessor accessor)
            : base(accessor)
        {
            _httpClientHelper = httpClientHelper;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
