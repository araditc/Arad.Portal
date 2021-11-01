using Arad.Portal.DataLayer.Contracts.General.Content;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ContentController : Controller
    {
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _accessor;
        public ContentController(IContentRepository contentRepository, IHttpContextAccessor accessor)
        {
            _contentRepository = contentRepository;  
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("{language}/blog/{**slug}")]
        public IActionResult Details(long slug)
        {
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            var entity = _contentRepository.FetchByCode(slug);
            return View(entity);
        }
    }
}
