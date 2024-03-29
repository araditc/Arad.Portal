﻿using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Menu;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Contracts.General.Domain;

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
            ILanguageRepository lanRepository, IMenuRepository menuRepository, IDomainRepository  domainRepository):base(accessor, domainRepository)
        {
            _groupRepository = groupRepository;
            _accessor = accessor;
            _languageRepository = lanRepository;
            _menuRepository = menuRepository;
            _domainName = base.DomainName;
        }

        [Route("{language}/group")]
        public IActionResult Index()
        {
            ViewData["DomainTitle"] = this.DomainTitle;
            ViewData["PageTitle"] = GeneralLibrary.Utilities.Language.GetString("design_ProductGroups");
            return View();
        }
        //[Route("group/{**slug}")]
        [Route("{language}/group/{**slug}")]
        public async Task<IActionResult> Details(string slug)
        {
           
            CommonViewModel model = new CommonViewModel();
            var path = _accessor.HttpContext.Request.Path.ToString();
            var lanIcon = path.Split("/")[1].ToLower();
            ViewData["DomainTitle"] = this.DomainTitle;
            ViewData["PageTitle"] = slug.Replace("-", " ");
            var langId = _languageRepository.FetchBySymbol(lanIcon);
            ViewData["CurLangId"] = langId;
            var id = _groupRepository.FetchByCode(slug);
            if(!string.IsNullOrWhiteSpace(id))
            {
                var grpSection = new GroupSection()
                {
                    ProductGroupId = id,
                    CountToSkip = 0,
                    CountToTake = 4,
                    DefaultLanguageId = langId
                };
                grpSection.GroupsWithProducts = await _groupRepository.AllGroupIdsWhichEndInProducts(_domainName);
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
            else
            {
                return Redirect($"~/{lanIcon}/ExceptionHandler/PageNotFound");
            }
            
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
