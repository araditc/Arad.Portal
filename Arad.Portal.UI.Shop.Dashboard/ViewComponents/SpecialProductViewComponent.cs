using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Arad.Portal.DataLayer.Entities.General.SliderModule;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.DesignStructure;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;

namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class SpecialProductViewComponent : ViewComponent
    {
        private readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILanguageRepository _lanRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IUserRepository _userRepository;

        public SpecialProductViewComponent(IProductRepository productRepository,ICurrencyRepository currencyRepository,
            IHttpContextAccessor accessor, ILanguageRepository lanRepository, IDomainRepository domainRepository, IUserRepository userRepository)
        {
            _productRepository = productRepository;
            _accessor = accessor;
            _lanRepository = lanRepository;
            _domainRepository = domainRepository;
            _currencyRepository = currencyRepository;
            _userRepository = userRepository;
        }

        public  IViewComponentResult Invoke(ModuleParameters moduleParameters)
        {
            var defaultCulture = _accessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            var defLangSymbol = defaultCulture.Split("|")[0][2..];
            CultureInfo currentCultureInfo = new(defLangSymbol, false);
            var ri = new RegionInfo(currentCultureInfo.LCID);
            var currencyPrefix = ri.ISOCurrencySymbol;
            var currencyDto = _currencyRepository.GetCurrencyByItsPrefix(currencyPrefix);
            ViewBag.CurrencySymbol = currencyDto.Symbol;

            var langId = _lanRepository.FetchBySymbol(defLangSymbol);
            ViewBag.CurLangId = langId;
            ViewBag.LoadAnimation = moduleParameters.LoadAnimation;
            ViewBag.LoadAnimationType = moduleParameters.LoadAnimationType;
            
            var lst = _productRepository.GetSpecialProducts(moduleParameters.Count.Value, currencyDto.CurrencyId, moduleParameters.ProductOrContentType.Value, 0, moduleParameters.DomainId);
            if(User.Identity.IsAuthenticated)
            {
                var userId = _accessor.HttpContext.User.GetUserId();
                var userFavoriteList = _userRepository.GetUserFavoriteList(userId, FavoriteType.Product);
                foreach (var item in lst)
                {
                    item.IsLikesByUserBefore = userFavoriteList.Any(_ => _.EntityId == item.ProductId);
                    #region check cookiepart for loggedUser
                    var userProductRateCookieName = $"{userId}_pp{item.ProductId}";
                    if (HttpContext.Request.Cookies[userProductRateCookieName] != null)
                    {
                        item.HasRateBefore = true;
                        item.PreRate = HttpContext.Request.Cookies[userProductRateCookieName];
                    }
                    else
                    {
                        item.HasRateBefore = false;
                    }
                    #endregion
                }
            }
            
            return moduleParameters.ProductTemplateDesign switch
            {
                ProductTemplateDesign.First => View("First", lst),
                _ => View(lst),
            };
        }
    }
}
