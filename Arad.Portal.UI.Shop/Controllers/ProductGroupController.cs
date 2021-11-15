using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Menu;
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
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IHttpContextAccessor _accessor;
        public ProductGroupController(IProductGroupRepository groupRepository, IHttpContextAccessor accessor,
            ILanguageRepository lanRepository, IMenuRepository menuRepository)
        {
            _groupRepository = groupRepository;
            _accessor = accessor;
            _languageRepository = lanRepository;
            _menuRepository = menuRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("{language}/group/{**slug}")]
        public IActionResult Details(long slug)
        {
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            var path = _accessor.HttpContext.Request.Path.ToString();
            var langCode = path.Split("/")[1].ToLower();

            var langId = _languageRepository.FetchBySymbol(langCode);
            ViewBag.CurLangId = langId;
           
            var model = _groupRepository.FetchByCode(slug);
            return View(model);
        }
    }
}
