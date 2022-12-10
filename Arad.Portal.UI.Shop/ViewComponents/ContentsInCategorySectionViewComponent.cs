﻿using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
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
    public class ContentsInCategorySectionViewComponent : ViewComponent
    {
        private readonly IContentCategoryRepository _categoryRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _languageRepository;
        public ContentsInCategorySectionViewComponent(IContentCategoryRepository categoryRepository, ILanguageRepository lanRepository,
            IHttpContextAccessor accessor, ICurrencyRepository currencyRepository, IDomainRepository domainRepository)
        {
            _accessor = accessor;
            _categoryRepository = categoryRepository;
            _currencyRepository = currencyRepository;
            _domainRepository = domainRepository;
            _languageRepository = lanRepository;
        }

        public IViewComponentResult Invoke(ContentsInCategorySection contentSection)
        {
            var result = new CommonViewModel();
            var domainName = $"{_accessor.HttpContext.Request.Host}";
            var domainEntity = _domainRepository.FetchByName(domainName, true).ReturnValue;
            result.ContentList = _categoryRepository.GetContentsInCategory(domainEntity.DomainId, 
                contentSection.ContentCategoryId, contentSection.CountToTake, contentSection.CountToSkip);

            var defaultCulture = _accessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            string defLangSymbol = "";
            if (defaultCulture != null)
            {
                defLangSymbol = defaultCulture.Split("|")[0][2..];
            }else
            {
                var defLangId = domainEntity.DefaultLanguageId;
                defLangSymbol = _languageRepository.FetchBySymbol(defLangId);
            }
            
            CultureInfo currentCultureInfo = new(defLangSymbol, false);
            var ri = new RegionInfo(currentCultureInfo.LCID);
            var currencyPrefix = ri.ISOCurrencySymbol;
            var currencyDto = _currencyRepository.GetCurrencyByItsPrefix(currencyPrefix);
            ViewBag.CurrencySymbol = currencyDto.Symbol;

            ViewBag.CurLangId = contentSection.DefaultLanguageId;
            return View(result);
        }
    }
}
