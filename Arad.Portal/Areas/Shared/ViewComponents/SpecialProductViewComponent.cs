using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;
using System.Linq;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Currency;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.DataLayer.Models.Shared.DesignStructure;
using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared.Product;
using System.Collections.Generic;
using System.Threading;

using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.Helpers.Shared;

using Language = Arad.Portal.DataLayer.Entities.General.Language.Language;

namespace Arad.Portal.Areas.Shared.ViewComponents;

public class SpecialProductViewComponent(
    IProductRepository productRepository,
    ICurrencyRepository currencyRepository,
    MinioHelper minioHelper,
    IHttpContextAccessor accessor,
    ILanguageRepository lanRepository,
    IDomainRepository domainRepository,
    IUserRepository userRepository,
    ControllerHelper controllerHelper)
    : ViewComponent
{

    public IViewComponentResult Invoke(ModuleParameters moduleParameters)
    {
        string? defaultCulture = accessor.HttpContext?.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
        string? defLangSymbol = defaultCulture?.Split("|")[0][2..];
        List<ProductOutputDto> lst = [];
        if (defLangSymbol == null)
        {
            return View("~/Areas/Shared/Views/Shared/Components/SpecialProduct/First.cshtml", lst);
        }

        CultureInfo currentCultureInfo = new(defLangSymbol, false);
        RegionInfo ri = new(currentCultureInfo.LCID);
        string currencyPrefix = ri.ISOCurrencySymbol;
        Currency currencyDto = controllerHelper.GetCurrencyByItsPrefix(currencyPrefix);
        ViewBag.CurrencySymbol = currencyDto.Symbol;

        string langId = controllerHelper.FetchLanguageBySymbol(defLangSymbol);
        ViewBag.CurLangId = langId;
        ViewBag.LoadAnimation = moduleParameters.LoadAnimation;
        ViewBag.LoadAnimationType = moduleParameters.LoadAnimationType;

        if (moduleParameters is not { Count: not null, ProductOrContentType: not null })
        {
            return View("~/Areas/Shared/Views/Shared/Components/SpecialProduct/First.cshtml", lst);
        }

        lst = controllerHelper.GetSpecialProducts(moduleParameters.Count.Value, currencyDto.Id, moduleParameters.ProductOrContentType.Value, 0, moduleParameters.DomainId);
        foreach (ProductOutputDto item in lst)
        {
            foreach (MultiLingualProperty obj in item.MultiLingualProperties)
            {
                obj.Name = obj.Name.Length > 80 ? obj.Name[..80] + "..." : obj.Name;
            }
            foreach (Image image in item.Images)
            {
                CultureInfo current = new("en-US")
                                      {
                                          DateTimeFormat = new()
                                                           {
                                                               Calendar = new GregorianCalendar()
                                                           }
                                      };
                Thread.CurrentThread.CurrentCulture = current;
                Domain domain = controllerHelper.GetCurrentUserDomain();
                string objectName = $"{domain.Id}/{image.ImageId}/{image.FileName.Replace(':', '-')}";
                (bool success, byte[] imageData) = minioHelper.GetObject("productimage", objectName).Result;
                if (success)
                {
                    image.Content = Convert.ToBase64String(imageData);

                }
                item.MainImageUrl = image.Content;
            }
        }
        if (User.Identity is { IsAuthenticated: true })
        {
            string? userId = accessor.HttpContext?.User.GetUserId();

            if (userId != null)
            {
                List<UserFavorites> userFavoriteList = controllerHelper.GetUserFavoriteList(userId, FavoriteType.Product);

                foreach (ProductOutputDto item in lst)
                {
                    if (userFavoriteList != null)
                    {
                        item.IsLikesByUserBefore = userFavoriteList.Any(f => f.EntityId == item.Id);
                    }
                    
                    #region check cookiepart for loggedUser
                    string userProductRateCookieName = $"{userId}_pp{item.Id}";
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
        }

        Language defLang = controllerHelper.GetDefaultLanguage();
        CultureInfo current2 = new(defLang.Symbol)
                               {
                                   DateTimeFormat = new()
                                                    {
                                                        Calendar = new GregorianCalendar()
                                                    }
                               };
        Thread.CurrentThread.CurrentCulture = current2;
        return moduleParameters.ProductTemplateDesign switch
               {
                   ProductTemplateDesign.First => View("~/Areas/Shared/Views/Shared/Components/SpecialProduct/First.cshtml", lst),
                   _ => View("~/Areas/Shared/Views/Shared/Components/SpecialProduct/First.cshtml", lst)
               };

    }
}