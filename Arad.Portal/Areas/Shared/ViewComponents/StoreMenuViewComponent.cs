using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Models.Shared;

using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Menu;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.Models.Shared;
using Arad.Portal.Helpers.Shared;


namespace Arad.Portal.Areas.Shared.ViewComponents;

public class StoreMenu(
    IHttpContextAccessor accessor,
    UserManager<ApplicationUser> userManager,
    IWebHostEnvironment env,
    IDomainRepository domainRepository,
    ILanguageRepository languageRepository,
    IMenuRepository menuRepository,
    IProductRepository productRepository,
    IContentRepository contentRepository,
    ControllerHelper controllerHelper)
    : ViewComponent
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IWebHostEnvironment _env = env;
    private readonly ILanguageRepository _languageRepository = languageRepository;
    private readonly IMenuRepository _menuRepository = menuRepository;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IContentRepository _contentRepository = contentRepository;

    public IViewComponentResult Invoke()
    {
        List<StoreMenuVm> menus = [];

        try
        {
            string langId = "";
            string cookieVal = accessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            if (cookieVal != null)
            {
                string symbol = cookieVal.Split("|")[0][2..];
                langId = controllerHelper.FetchLanguageBySymbol(symbol.ToLower());
            }

            string domainName = controllerHelper.GetCurrentDomainName();
            Domain domain = domainRepository.FirstOrDefault(c => c.DomainName == "https://" + domainName);
            if (langId == "")
            {
                langId = domain.DefaultLanguageId;
            }


            if (domain != null)
            {
                menus = controllerHelper.StoreList(domain.Id, langId, false);
            }
        }
        catch (Exception e)
        {
            // ignored
        }

        return View("~/Areas/Shared/Views/Shared/Components/StoreMenu/Default.cshtml", menus);
    }
}