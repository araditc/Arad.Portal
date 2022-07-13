using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Menu;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class StoreMenuModule : ViewComponent
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IMenuRepository _menuRepository;
        public StoreMenuModule(IHttpContextAccessor accessor, UserManager<ApplicationUser> userManager, 
            IDomainRepository domainRepository, ILanguageRepository languageRepository, IMenuRepository menuRepository)
        {
            _accessor = accessor;
            _userManager = userManager;
            _domainRepository = domainRepository;
            _languageRepository = languageRepository;
            _menuRepository = menuRepository;
        }
        public  IViewComponentResult Invoke()
        {
            var menues = new List<StoreMenuVM>();
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            //??? for testing
            //var domainName = "localhost:3214";
            var result = _domainRepository.FetchByName(domainName, true);
            var domainEntity = result.ReturnValue;
           
           
            try
            {
                var languageId = domainEntity.DefaultLanguageId;
                if (string.IsNullOrWhiteSpace(domainEntity.DefaultLanguageId))
                {
                    var cookieVal = _accessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
                    string symbol = cookieVal.Split("|")[0][2..];
                    languageId = _languageRepository.FetchBySymbol(symbol.ToLower());
                }

                menues = _menuRepository.StoreList(domainEntity.DomainId, languageId, true);
            }
            catch (Exception e)
            {
            }
            return View(menues);
        }
    }
}
