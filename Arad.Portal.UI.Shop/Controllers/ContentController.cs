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
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("{language}/blog/{**slug}")]
        public IActionResult Details(long slug)
        {
            var entity = _contentRepository.FetchByCode(slug);
            return View(entity);
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
