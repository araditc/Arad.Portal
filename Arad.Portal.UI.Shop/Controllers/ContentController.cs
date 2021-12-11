using Arad.Portal.DataLayer.Contracts.General.Content;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ContentController : BaseController
    {
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _accessor;
        public ContentController(IContentRepository contentRepository, IHttpContextAccessor accessor):base(accessor)
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
            var entity = _contentRepository.FetchByCode(slug);
            return View(entity);
        }
    }
}
