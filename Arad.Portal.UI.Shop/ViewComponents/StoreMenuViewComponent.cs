using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Menu;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.ViewComponents
{
    public class StoreMenu : ViewComponent
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IMenuRepository _menuRepository;
        public StoreMenu(IHttpContextAccessor accessor, UserManager<ApplicationUser> userManager, 
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
            var result = _domainRepository.FetchByName(domainName);
            var domainEntity = result.ReturnValue;

            domainName = _accessor.HttpContext.Request.Host.ToString();
            if (domainName.ToString().ToLower().StartsWith("localhost"))
            {
                domainName = _accessor.HttpContext.Request.Host.ToString().Substring(0, 9);
            }
            try
            {
                var defLangId = _accessor.HttpContext.Request.Cookies[$"defLang{domainName}"];
                menues = _menuRepository.StoreList(domainEntity.DomainId, defLangId);
            }
            catch (Exception e)
            {
            }
            return View(menues);
        }
    }
}
