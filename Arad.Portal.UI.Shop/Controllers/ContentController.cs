 using Arad.Portal.DataLayer.Contracts.General.Content;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Arad.Portal.DataLayer.Contracts.General.Language;
using System.Web;
using System.Collections.Specialized;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Entities.General.User;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Models.Content;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Microsoft.AspNetCore.Authorization;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ContentController : BaseController
    {
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILanguageRepository _lanRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;

        public ContentController(IContentRepository contentRepository,
            IDomainRepository domainRepository,
            ILanguageRepository lanRepository,
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepository,
            IHttpContextAccessor accessor):base(accessor, domainRepository)
        {
            _contentRepository = contentRepository;
            _accessor = accessor;
            _domainRepository = domainRepository;
            _lanRepository = lanRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        [Route("{language?}/blog")]
        public IActionResult Index()
        {
            var domainName = this.DomainName;
            ViewData["DomainTitle"] = this.DomainTitle;
            ViewData["PageTitle"] = Language.GetString("design_Articles");
            var domainEntity = _domainRepository.FetchByName(domainName, false).ReturnValue;
            string lanSymbol = "";
            string lanId = "";
            if(CultureInfo.CurrentCulture.Name != null)
            {
                lanSymbol = CultureInfo.CurrentCulture.Name;
                lanId = _lanRepository.FetchBySymbol(lanSymbol);
            }
            else
            {
                lanId = domainEntity.DefaultLanguageId;
            }
            
            //var cookieName = CookieRequestCultureProvider.DefaultCookieName;
            
            var lst = _contentRepository.GetAllBlogList(Request.QueryString.ToString(), domainEntity.DomainId, lanId);
            foreach (var item in lst.Items)
            {
                item.DesiredImageUrl = item.Images.Any(_ => _.IsMain) ? item.Images.FirstOrDefault(_ => _.IsMain).Url : "";
            }

            return View("Index",lst);
        }


        [AllowAnonymous]
        [Route("{language?}/Articles")]
        public IActionResult Articles()
        {

            var result = _domainRepository.FetchByName(DomainName, false);
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            var lanId = _lanRepository.FetchBySymbol(lanIcon);
            ViewData["DomainTitle"] = this.DomainTitle;
            ViewData["PageTitle"] = Language.GetString("design_Articles");

            if (result.Succeeded)
            {
                if (!result.ReturnValue.IsMultiLinguals) //single language
                {
                    var lan = result.ReturnValue.DefaultLanguageId;
                    var lanEntity = _lanRepository.FetchLanguage(lan);
                    Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(lanEntity.Symbol))
                    , new CookieOptions()
                    {
                        Expires = DateTimeOffset.Now.AddYears(1),
                        Domain = result.ReturnValue.DomainName
                    });
                }


                if (result.ReturnValue.BlogPageDesign.Any(_ => _.LanguageId == lanId))
                {
                    var m = result.ReturnValue.BlogPageDesign.FirstOrDefault(_ => _.LanguageId == lanId);
                    return View(m.MainPageContainerPart);
                }
                else
                {
                    return View((new DataLayer.Models.DesignStructure.MainPageContentPart()));
                }
            }
            else
            {
                return View(new DataLayer.Models.DesignStructure.MainPageContentPart());
            }


        }

        [Route("{language}/blog/{**slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            ViewData["DomainTitle"] = this.DomainTitle;
           
            string userId = "";
            userId = HttpContext.User.Identity.IsAuthenticated ? User.GetUserId() : "";
            var lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
            var entity = _contentRepository.FetchByCode(slug);
            if(entity != null)
            {
                var updateVisitCount = await _contentRepository.UpdateVisitCount(entity.ContentId);
                if(entity.IsSidebarContentsShowing)
                {
                    var sidebars = _contentRepository.GetContentInCategory(entity.SidebarContentCount?? 3, 
                                      DataLayer.Entities.General.DesignStructure.ProductOrContentType.Newest, entity.ContentCategoryId,  false);
                    foreach (var item in sidebars)
                    {
                        item.DesiredImageUrl = item.Images.Any(_ => _.IsMain) ? item.Images.FirstOrDefault(_ => _.IsMain).Url : $"/imgs/NoImage21.jpg";
                    }

                    ViewBag.Sidbars = sidebars;
                }
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    #region check cookiepart for loggedUser
                    var userProductRateCookieName = $"{userId}_cc{entity.ContentId}";
                    if (HttpContext.Request.Cookies[userProductRateCookieName] != null)
                    {
                        ViewBag.HasRateBefore = true;
                        ViewBag.PreRate = HttpContext.Request.Cookies[userProductRateCookieName];
                    }
                    else
                    {
                        ViewBag.HasRateBefore = false;
                    }
                    #endregion
                    #region FavoriteList
                    var userEntity = await _userManager.FindByIdAsync(userId);
                    var userFavoriteList = _userRepository.GetUserFavoriteList(userId, FavoriteType.Content);

                    if (userFavoriteList.Any(_ => _.EntityId == entity.ContentId))
                    {
                        ViewBag.Like = true;
                    }
                    else
                    {
                        ViewBag.Like = false;
                    }
                    #endregion
                }
                ViewData["PageTitle"] = entity.Title;
                ViewBag.LanIcon = lanIcon;
                return View(entity);
            }else
            {
                return Redirect($"~/{lanIcon}/ExceptionHandler/PageNotFound");
            }
           
        }

        
    }
}
