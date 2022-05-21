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
using Microsoft.AspNetCore.Hosting;


namespace Arad.Portal.UI.Shop.Dashboard.ViewComponents
{
    public class StoreMenu : ViewComponent
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDomainRepository _domainRepository;
        private readonly IWebHostEnvironment _env;
        private readonly ILanguageRepository _languageRepository;
        private readonly IMenuRepository _menuRepository;
        public StoreMenu(IHttpContextAccessor accessor, UserManager<ApplicationUser> userManager, 
            IWebHostEnvironment env,
            IDomainRepository domainRepository, ILanguageRepository languageRepository, IMenuRepository menuRepository)
        {
            _accessor = accessor;
            _userManager = userManager;
            _domainRepository = domainRepository;
            _languageRepository = languageRepository;
            _menuRepository = menuRepository;
            _env = env;
        }
        public  IViewComponentResult Invoke()
        {
            var menues = new List<StoreMenuVM>();
            string domainName;
            
            //string domainName = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            //if (domainName.ToString().ToLower().StartsWith("localhost"))
            //{
            //    //prevent port of localhost
            //    domainName = HttpContext.Request.Host.ToString().Substring(0, 9);
            //}
            

            try
            {
                var cookieVal = _accessor.HttpContext.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
                string symbol = cookieVal.Split("|")[0][2..];
                var langId = _languageRepository.FetchBySymbol(symbol.ToLower());
                //if (_env.EnvironmentName == "Development")
                //{
                //    domainName = "http://localhost:17951";
                //}
                //else
                //{
                    domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
                //}
                var result = _domainRepository.FetchByName(domainName);
                var domainEntity = result.ReturnValue;

                menues = _menuRepository.StoreList(domainEntity.DomainId, langId);
            }
            catch (Exception e)
            {
            }
            return View(menues);
        }
    }
}
