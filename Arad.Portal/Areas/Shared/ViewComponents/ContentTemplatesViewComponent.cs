using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Shared.DesignStructure;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared.Content;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.User;

namespace Arad.Portal.Areas.Shared.ViewComponents;

public class ContentTemplatesViewComponent(
    IContentRepository contentRepository,
    IHttpContextAccessor accessor,
    ILanguageRepository lanRepository,
    IDomainRepository domainRepository,
    MinioHelper minioHelper,
    ControllerHelper controllerHelper)
    : ViewComponent
{
    private readonly IContentRepository _contentRepository = contentRepository;
    private readonly ILanguageRepository _lanRepository = lanRepository;

    public IViewComponentResult Invoke(ModuleParameters moduleParameters)
    {
        Task<ApplicationUser> userDb = controllerHelper.GetCurrentUser();
        string domainName = controllerHelper.GetCurrentDomainName();
        Domain domain = domainRepository.FirstOrDefault(c => c.DomainName == "https://" + domainName);
        List<ContentGlance> lst = [];
        string defLangSymbol = "";

        Language defLanguage = controllerHelper.GetDefaultLanguage();
        if (User.Identity.IsAuthenticated)
        {


            if (!string.IsNullOrEmpty(userDb.Result.Profile.DefaultLanguageId))
            {
                defLangSymbol = controllerHelper.FetchLanguage(userDb.Result.Profile.DefaultLanguageId).Symbol;
                Thread.CurrentThread.CurrentCulture = new(defLangSymbol, false);
            }
        }
        else if (defLanguage != null)
        {
            defLangSymbol = defLanguage.Symbol;
            Thread.CurrentThread.CurrentCulture = new(defLangSymbol, false);
        }
        else
        {
            string defaultCulture = accessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (defaultCulture != null)
            {
                defLangSymbol = defaultCulture.Split('|')[0][2..];
            }
            else
            {
                string defLangId = domain.DefaultLanguageId;
                defLangSymbol = controllerHelper.FetchLanguageBySymbol(defLangId);
            }
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
        }

        string langId = controllerHelper.FetchLanguageBySymbol(defLangSymbol);
        ViewBag.CurLangId = langId;

        ViewBag.LoadAnimation = moduleParameters.LoadAnimation;
        ViewBag.LoadAnimationType = moduleParameters.LoadAnimationType;

        lst = controllerHelper.GetSpecialContent(
            moduleParameters.Count.Value,
            moduleParameters.ProductOrContentType.Value,
            moduleParameters.SelectionType.Value,
            moduleParameters.CatId,
            moduleParameters.SelectedIds,
            false,
            domain.Id);

        Language defLang = controllerHelper.GetDefaultLanguage();
        CultureInfo current = new("en-US")
        {
            DateTimeFormat = { Calendar = new GregorianCalendar() }
        };
        CultureInfo current2 = new(defLang.Symbol)
        {
            DateTimeFormat = { Calendar = new GregorianCalendar() }
        };

        switch (moduleParameters.ContentTemplateDesign)
        {
            case ContentTemplateDesign.First:
                SetImageForFirstTemplate(lst, domain);
                return View("~/Areas/Shared/Views/Shared/Components/ContentTemplates/First.cshtml", lst);

            case ContentTemplateDesign.Second:
                SetImageForSecondTemplate(lst, domain);
                return View("~/Areas/Shared/Views/Shared/Components/ContentTemplates/Second.cshtml", lst);

            case ContentTemplateDesign.Third:
                SetImageForThirdTemplate(lst, domain);
                return View("~/Areas/Shared/Views/Shared/Components/ContentTemplates/Third.cshtml", lst);

            case ContentTemplateDesign.Forth:
                SetImageForForthTemplate(lst, domain);
                return View("~/Areas/Shared/Views/Shared/Components/ContentTemplates/Forth.cshtml", lst);

            case ContentTemplateDesign.Fifth:
                return View("~/Areas/Shared/Views/Shared/Components/ContentTemplates/Fifth.cshtml", lst);

            case ContentTemplateDesign.Sixth:
                SetImageForSixthTemplate(lst, domain);
                return View("~/Areas/Shared/Views/Shared/Components/ContentTemplates/Sixth.cshtml", lst);

            case ContentTemplateDesign.Seventh:
                SetImageForSeventhTemplate(lst, domain);
                return View("~/Areas/Shared/Views/Shared/Components/ContentTemplates/Seventh.cshtml", lst);

            case ContentTemplateDesign.SliderWithSubtitle:
                SetImageForSliderWithSubtitleTemplate(lst, domain);
                return View("~/Areas/Shared/Views/Shared/Components/ContentTemplates/SliderWithSubtitle.cshtml", lst);

            default:
                return View(lst);
        }
    }

    private void SetImageForFirstTemplate(List<ContentGlance> lst, Domain domain)
    {
        foreach (ContentGlance item in lst)
        {
            Thread.CurrentThread.CurrentCulture = new("en-US");
            SetImage(item, domain, ImageRatio.Square, Enums.ImageTemplateType.Square, "/imgs/NoImage.png");
            Thread.CurrentThread.CurrentCulture = new(controllerHelper.GetDefaultLanguage().Symbol);
        }
    }

    private void SetImageForSecondTemplate(List<ContentGlance> lst, Domain domain)
    {
        for (int i = 0; i < 4; i++)
        {
            Thread.CurrentThread.CurrentCulture = new("en-US");
            if (i == 0 || i == 3)
            {
                SetImage(lst[i], domain, ImageRatio.TwoToOne, Enums.ImageTemplateType.X21Andx41, "/imgs/NoImage21.jpg");
            }
            else
            {
                SetImage(lst[i], domain, ImageRatio.FourToOne, Enums.ImageTemplateType.X21Andx41, "/imgs/NoImage41.jpg");
            }
            Thread.CurrentThread.CurrentCulture = new(controllerHelper.GetDefaultLanguage().Symbol);
        }
    }

    private void SetImageForThirdTemplate(List<ContentGlance> lst, Domain domain)
    {
        for (int i = 0; i < 8; i++)
        {
            Thread.CurrentThread.CurrentCulture = new("en-US");
            if (i < 4)
            {
                SetImage(lst[i], domain, ImageRatio.TwoToOne, Enums.ImageTemplateType.X21, "/imgs/NoImage21.jpg");
            }
            else
            {
                SetImage(lst[i], domain, ImageRatio.Square, Enums.ImageTemplateType.Square, "/imgs/NoImage.png");
            }
            Thread.CurrentThread.CurrentCulture = new(controllerHelper.GetDefaultLanguage().Symbol);
        }
    }

    private void SetImageForForthTemplate(List<ContentGlance> lst, Domain domain)
    {
        foreach (ContentGlance item in lst)
        {
            Thread.CurrentThread.CurrentCulture = new("en-US");
            SetImage(item, domain, ImageRatio.Square, Enums.ImageTemplateType.Circular, "/imgs/NoImage.png");
            Thread.CurrentThread.CurrentCulture = new(controllerHelper.GetDefaultLanguage().Symbol);
        }
    }

    private void SetImageForSixthTemplate(List<ContentGlance> lst, Domain domain)
    {
        foreach (ContentGlance item in lst)
        {
            Thread.CurrentThread.CurrentCulture = new("en-US");
            SetImage(item, domain, ImageRatio.Square, Enums.ImageTemplateType.HexagonalLogo, "/imgs/NoImage.png");
            Thread.CurrentThread.CurrentCulture = new(controllerHelper.GetDefaultLanguage().Symbol);
        }
    }

    private void SetImageForSeventhTemplate(List<ContentGlance> lst, Domain domain)
    {
        foreach (ContentGlance item in lst)
        {
            Thread.CurrentThread.CurrentCulture = new("en-US");
            SetImage(item, domain, ImageRatio.Square, Enums.ImageTemplateType.HexagonalLogo, "/imgs/NoImage.png");
            Thread.CurrentThread.CurrentCulture = new(controllerHelper.GetDefaultLanguage().Symbol);
        }
    }

    private void SetImageForSliderWithSubtitleTemplate(List<ContentGlance> lst, Domain domain)
    {
        foreach (ContentGlance item in lst)
        {
            Thread.CurrentThread.CurrentCulture = new("en-US");
            SetImage(item, domain, ImageRatio.Square, Enums.ImageTemplateType.SliderWithSubtitle, "/imgs/NoImage.png");
            item.ImageTitle = item.Images.FirstOrDefault()?.Title;
            Thread.CurrentThread.CurrentCulture = new(controllerHelper.GetDefaultLanguage().Symbol);
        }
    }

    private void SetImage(ContentGlance item, Domain domain, ImageRatio ratio, Enums.ImageTemplateType templateType, string noImagePath)
    {
        Image image = item.Images.FirstOrDefault(i => i.ImageRatio == ratio && i.ImageTemplateTypes.Contains(templateType));
        if (image != null)
        {
            string objectName = "";

            objectName = $"{domain.Id}/{image.ImageId}/{image.FileName.Replace(':', '-')}";
            (bool success, byte[] imageData) = minioHelper.GetObject("contentimage", objectName).Result;
            if (success)
            {
                image.Content = Convert.ToBase64String(imageData);
                item.DesiredImageUrl = image.Content;
            }
            else
            {
                item.DesiredImageUrl = noImagePath;
            }
        }
        else
        {
            item.DesiredImageUrl = noImagePath;
        }
    }
}