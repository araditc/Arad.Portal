using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ContentCategoryController : Controller
    {
        private readonly IContentCategoryRepository _categoryRepository;
        private readonly IHttpContextAccessor _accessor;
        public ContentCategoryController(IContentCategoryRepository categoryRepository,
            IHttpContextAccessor accessor)
        {
            _categoryRepository = categoryRepository;
            _accessor = accessor;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("{language}/category/{**slug}")]
        public IActionResult Details(long slug)
        {
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            var entity = _categoryRepository.FetchByCode(slug);
            return View(entity);
        }
    }
}
