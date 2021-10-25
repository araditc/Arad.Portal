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
        [Route("/blogCategory/{slug}")]
        public IActionResult Details(string slug)
        {
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            var entity = _categoryRepository.FetchBySlug(slug, domainName);
            return View(entity);
        }
    }
}
