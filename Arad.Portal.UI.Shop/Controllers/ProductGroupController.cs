using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ProductGroupController : Controller
    {
        private readonly IProductGroupRepository _groupRepository;
        private readonly IHttpContextAccessor _accessor;
        public ProductGroupController(IProductGroupRepository groupRepository, IHttpContextAccessor accessor)
        {
            _groupRepository = groupRepository;
            _accessor = accessor;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("{language}/group/{**slug}")]
        public IActionResult Details(long slug)
        {
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            var model = _groupRepository.FetchByCode(slug);
            return View(model);
        }
    }
}
