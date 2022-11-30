﻿ using Arad.Portal.DataLayer.Contracts.General.Content;
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

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ContentController : BaseController
    {
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILanguageRepository _lanRepository;
        private readonly IDomainRepository _domainrepository;
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
            _domainrepository = domainRepository;
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
            var domainEntity = _domainrepository.FetchByName(domainName, false).ReturnValue;
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

        [Route("{language}/blog/{**slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            ViewData["DomainTitle"] = this.DomainTitle;
            var isLoggedUser = HttpContext.User.Identity.IsAuthenticated;
            string userId = "";
            userId = isLoggedUser ? User.GetUserId() : "";
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
                if (isLoggedUser)
                {
                    #region check cookiepart for loggedUser
                    var userProductRateCookieName = $"{userId}_c{entity.ContentId}";
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

        [HttpPost]
        public async Task<IActionResult> Rate([FromQuery] string contentId, [FromQuery] int score, [FromQuery] bool isNew)
        {
            var userId = User.GetUserId();
            string prevRate = "";
            var userProductRateCookieName = $"{userId}_cc{contentId}";
            if (!isNew)//the user has rated before
            {
                prevRate = HttpContext.Request.Cookies[userProductRateCookieName];
            }
            int preS = !string.IsNullOrWhiteSpace(prevRate) ? Convert.ToInt32(prevRate) : 0;

            var res = await _contentRepository.RateContent(contentId, score,
                    isNew, preS);
            if (res.Succeeded)
            {
                //set its related cookie
                return
                    Json(new
                    {
                        status = "Succeed",
                        like = res.ReturnValue.LikeRate,
                        dislike = res.ReturnValue.DisikeRate,
                        half = res.ReturnValue.HalfLikeRate
                    });
            }
            else
            {
                return Json(new { status = "error" });
            }
        }
    }
}
