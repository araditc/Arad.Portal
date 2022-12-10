using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
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
        private readonly ILanguageRepository _lanRepository;
        private readonly IDomainRepository _DomainRepository;
        public ProductsInGroupSectionViewComponent(IProductGroupRepository groupRepository,
            ILanguageRepository lanRepository,IDomainRepository domainRepository,
            IHttpContextAccessor accessor, ICurrencyRepository currencyRepository)
        {
            _accessor = accessor;
            _groupRepository = groupRepository;
            _currencyRepository = currencyRepository;
            _lanRepository = lanRepository;
            _DomainRepository = domainRepository;
        }

        public IViewComponentResult Invoke(ProductsInGroupSection productSection)
        {
            var result = new CommonViewModel();
            var domainName = $"{_accessor.HttpContext.Request.Host}";
            var domainEntity = _DomainRepository.FetchByName(domainName, true).ReturnValue;
            result.ProductList = _groupRepository
                .GetLatestProductInThisGroup(domainName, productSection.ProductGroupId, productSection.CountToTake, productSection.CountToSkip);
            var defaultCulture = _accessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            string defLangSymbol;
            if (defaultCulture != null)
            {
                defLangSymbol = defaultCulture.Split("|")[0][2..];
            }
            else
            {
                var defLangId = domainEntity.DefaultLanguageId;
                defLangSymbol = _lanRepository.FetchBySymbol(defLangId);
            }
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
