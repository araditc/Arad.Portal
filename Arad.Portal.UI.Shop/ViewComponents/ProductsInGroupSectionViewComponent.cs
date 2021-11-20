using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents

{
    public class ProductsInGroupSectionViewComponent : ViewComponent
    {
        private readonly IProductGroupRepository _groupRepository;
        private readonly IHttpContextAccessor _accessor;
        public ProductsInGroupSectionViewComponent(IProductGroupRepository groupRepository, IHttpContextAccessor accessor)
        {
            _accessor = accessor;
            _groupRepository = groupRepository;
        }

        public IViewComponentResult Invoke(ProductsInGroupSection productSection)
        {
            var result = new CommonViewModel();
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            result.ProductList = _groupRepository
                .GetLatestProductInThisGroup(domainName, productSection.ProductGroupId, productSection.CountToTake, productSection.CountToSkip);

            ViewBag.CurLangId = productSection.DefaultLanguageId;
            return View(result);
        }
    }
}
