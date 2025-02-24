using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Models.Shared.DesignStructure;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;

namespace Arad.Portal.Controllers.Base;

public class HomeController(
    IHttpContextAccessor accessor,
    IDomainRepository domainRepository,
    ILanguageRepository languageRepository,
    ControllerHelper controllerHelper)
    : BaseController(accessor, domainRepository, languageRepository)
{

    [AllowAnonymous]
    public IActionResult Index()
    {

        Result<Domain> result = controllerHelper.FetchDomainByName(DomainName, false);
        ViewBag.IsAnchor = result.ReturnValue.IsAnchor;
        ViewData["DomainTitle"] = DomainTitle;
        ViewData["PageTitle"] = UtilityLanguage.GetString("design_HomePage");
        if (result.Succeeded)
        {
            if (!result.ReturnValue.IsMultiLinguals) //single language
            {
                string lan = result.ReturnValue.DefaultLanguageId;
                DataLayer.Entities.General.Language.Language lanEntity = controllerHelper.FetchLanguage(lan);
                Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                                        CookieRequestCultureProvider.MakeCookieValue(new(lanEntity.Symbol))
                                        , new()
                                        {
                                            Expires = DateTimeOffset.Now.AddYears(1),
                                            Domain = result.ReturnValue.DomainName
                                        });
            }

            List<PageDesignContent> homePageDesignList = result.ReturnValue.HomePageDesign.ToList();
            switch (homePageDesignList.Count)
            {
                case >= 2:
                    {
                        foreach (PageDesignContent homeItem in homePageDesignList)
                        {
                            string defLangId = result.ReturnValue.DefaultLanguageId;
                            if (User.Identity is { IsAuthenticated: true })
                            {
                                Task<ApplicationUser> userEntity = controllerHelper.GetCurrentUser();
                                

                                if (homePageDesignList.All(c => c.LanguageId != defLangId))
                                {
                                    return View(new PageDesignContent());
                                }

                                {
                                    PageDesignContent m = homePageDesignList.FirstOrDefault(c => c.LanguageId == defLangId);
                                    return View(m);
                                }

                            }

                            if (homePageDesignList.Any(c => c.LanguageId == defLangId))
                            {
                                PageDesignContent m = homePageDesignList.FirstOrDefault(c => c.LanguageId == defLangId);
                                return View(m);
                            }
                            else
                            {
                                return View(new PageDesignContent());
                            }


                        }

                        break;
                    }

                case 1 when homePageDesignList.FirstOrDefault() != null:
                    {
                        PageDesignContent m = homePageDesignList.FirstOrDefault();
                        return View(m);
                    }

                case 1:
                    return View(new PageDesignContent());
            }

        }
        else
        {
            return View(new PageDesignContent());
        }
        return View(new PageDesignContent());

    }
    public IActionResult Privacy()
    {
        return View();
    }



    //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}