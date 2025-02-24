using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.ContentCategory;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Currency;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared.Content;
using Arad.Portal.Models.UI;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Language;

using Microsoft.Extensions.Configuration;

using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.ViewComponents;

public class ContentsInCategorySectionViewComponent(
    IMapper mapper,
    IHttpContextAccessor accessor,
    IContentRepository contentRepository,
    MinioHelper minioHelper,
    IConfiguration configuration,
    ControllerHelper controllerHelper)
    : ViewComponent
{

    public IViewComponentResult Invoke(ContentsInCategorySection contentSection)
    {
        CommonViewModel result = new();
        string domainName = $"{accessor.HttpContext.Request.Host}";
        Domain domainEntity = controllerHelper.FetchDomainByName(domainName, true).ReturnValue;

        List<DataLayer.Entities.General.Content.Content> lst = new();
        if (contentSection.CountToTake != null)
        {
            lst = contentRepository
                  .GetList(c => c.ContentCategoryId == contentSection.ContentCategoryId && c.AssociatedDomainId == domainEntity.Id && !c.IsDeleted)
                  .OrderByDescending(c => c.CreationDate)
                  .Skip(0)
                  .ToList();
        }
        else
        {
            lst = contentRepository.GetList(c => c.ContentCategoryId == contentSection.ContentCategoryId && c.AssociatedDomainId == domainEntity.Id && !c.IsDeleted)
                                    .OrderByDescending(c => c.CreationDate).ToList();
        }
        List<ContentViewModel> res = mapper.Map<List<ContentViewModel>>(lst);

        foreach (ContentViewModel content in res)
        {
            EntityRate r = DataLayer.Helpers.Utilities.ConvertPopularityRate(content.TotalScore ?? 0, content.ScoredCount ?? 0);
            content.LikeRate = r.LikeRate;
            content.DisikeRate = r.DisikeRate;
            content.HalfLikeRate = r.HalfLikeRate;

            IEnumerable<Image> images = content.Images.Where(c => c.IsMain);

            foreach (Image image in images)
            {
                CultureInfo current = new("en-US") { DateTimeFormat = new() { Calendar = new GregorianCalendar() } };
                Thread.CurrentThread.CurrentCulture = current;
                string objectName = $"{domainEntity.Id}/{image.ImageId}/{image.FileName.Replace(':', '-')}";
                (bool success, byte[] imageData) = minioHelper.GetObject("contentimage", objectName).Result;

                if (success)
                {
                    image.Content = Convert.ToBase64String(imageData);

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
        result.ContentList = res;

        string defaultCulture = accessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
        string defLangSymbol = "";
        if (defaultCulture != null)
        {
            defLangSymbol = defaultCulture.Split("|")[0][2..];
        }
        else
        {
            string defLangId = domainEntity.DefaultLanguageId;
            defLangSymbol = controllerHelper.FetchLanguageBySymbol(defLangId);
        }

        CultureInfo currentCultureInfo = new(defLangSymbol, false);
        RegionInfo ri = new RegionInfo(currentCultureInfo.LCID);
        string currencyPrefix = ri.ISOCurrencySymbol;
        Currency currencyDto = controllerHelper.GetCurrencyByItsPrefix(currencyPrefix);
        ViewBag.CurrencySymbol = currencyDto.Symbol;
        ViewBag.FontFamily = configuration["SiteSettings:FontFamily"] ?? "Arial, sans-serif";
        ViewBag.CurLangId = contentSection.DefaultLanguageId;

        return View(result);
    }
}