using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents

{
    public class ProductsInGroupSectionViewComponent : ViewComponent
    {
        private readonly IProductGroupRepository _groupRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ICurrencyRepository _currencyRepository;
        public ProductsInGroupSectionViewComponent(IProductGroupRepository groupRepository,
            IHttpContextAccessor accessor, ICurrencyRepository currencyRepository)
        {
            _accessor = accessor;
            _groupRepository = groupRepository;
            _currencyRepository = currencyRepository;
        }

        public IViewComponentResult Invoke(ProductsInGroupSection productSection)
        {
            var result = new CommonViewModel();
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            result.ProductList = _groupRepository
                .GetLatestProductInThisGroup(domainName, productSection.ProductGroupId, productSection.CountToTake, productSection.CountToSkip);
            var defaultCulture = _accessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            var defLangSymbol = defaultCulture.Split("|")[0][2..];
            CultureInfo currentCultureInfo = new(defLangSymbol, false);
            var ri = new RegionInfo(currentCultureInfo.LCID);
            var currencyPrefix = ri.ISOCurrencySymbol;
            var currencyDto = _currencyRepository.GetCurrencyByItsPrefix(currencyPrefix);
            ViewBag.CurrencySymbol = currencyDto.Symbol;

            ViewBag.CurLangId = productSection.DefaultLanguageId;
            return View(result);
        }
    }
}
