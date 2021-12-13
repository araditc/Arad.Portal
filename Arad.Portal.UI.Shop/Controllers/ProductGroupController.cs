using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Menu;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ProductGroupController : BaseController
    {
        private readonly IProductGroupRepository _groupRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly string _domainName ;
        public ProductGroupController(IProductGroupRepository groupRepository, IHttpContextAccessor accessor,
            ILanguageRepository lanRepository, IMenuRepository menuRepository):base(accessor)
        {
            _groupRepository = groupRepository;
            _accessor = accessor;
            _languageRepository = lanRepository;
            _menuRepository = menuRepository;
            _domainName = base.DomainName;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("{language}/group/{**slug}")]
        public async Task<IActionResult> Details(long slug)
        {
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            CommonViewModel model = new CommonViewModel();
            var path = _accessor.HttpContext.Request.Path.ToString();
            var langCode = path.Split("/")[1].ToLower();

            var langId = _languageRepository.FetchBySymbol(langCode);
            ViewData["CurLangId"] = langId;
            var id = _groupRepository.FetchByCode(slug);
            var grpSection = new GroupSection()
            {
                ProductGroupId = id,
                CountToSkip = 0,
                CountToTake = 4,
                DefaultLanguageId = langId
            };
            grpSection.GroupsWithProducts = await _groupRepository.AllGroupIdsWhichEndInProducts(domainName);
            model.GroupSection = grpSection;

            var proSection = new ProductsInGroupSection()
            {
                ProductGroupId = id,
                CountToSkip = 0,
                CountToTake = 4,
                DefaultLanguageId = langId
            };
            model.ProductInGroupSection = proSection;
            //var model = _groupRepository.FetchByCode(slug);
            return View(model);
        }

        [HttpPost]
        public IActionResult GetMyGroupVC(GroupSection groupSection)
        {

            return ViewComponent("GroupSection", groupSection);
        }

        public IActionResult GetProductsInGroupVC(ProductsInGroupSection productsSection)
        {
            return ViewComponent("ProductsInGroupSection", productsSection);
        }

    }
}
