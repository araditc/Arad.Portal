using Arad.Portal.DataLayer.Contracts.General.Content;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ContentController : BaseController
    {
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _accessor;

        public ContentController(IContentRepository contentRepository,
            IWebHostEnvironment env,
            IHttpContextAccessor accessor):base(accessor, env)
        {
            _contentRepository = contentRepository;
            _accessor = accessor;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("{language}/blog/{**slug}")]
        public IActionResult Details(string slug)
        {
            var isLoggedUser = HttpContext.User.Identity.IsAuthenticated;
            string userId = "";
            userId = isLoggedUser ? HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value : "";
            var lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
            var entity = _contentRepository.FetchByCode(slug);
            if(entity != null)
            {
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
                }

                return View(entity);
            }else
            {
                return Redirect($"~/{lanIcon}/ExceptionHandler/PageNotFound");
            }
           
        }

        [HttpPost]
        public async Task<IActionResult> Rate([FromQuery] string contentId, [FromQuery] int score, [FromQuery] bool isNew)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value;
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
