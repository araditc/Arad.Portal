using Arad.Portal.DataLayer.Contracts.General.Comment;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Comment;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class ProductController : BaseController
    {
        private readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILanguageRepository _lanRepository;
        private readonly IDomainRepository _domainRepository;
        //private readonly EnyimMemcachedMethods<DataLayer.Entities.Shop.Transaction.Transaction> _enyimMemcachedMethods;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICommentRepository _commentRepository;
        private readonly string _domainName;

        public ProductController(IProductRepository productRepository, IHttpContextAccessor accessor,
            UserManager<ApplicationUser> userManager,
            //EnyimMemcachedMethods<DataLayer.Entities.Shop.Transaction.Transaction> enyimMemcachedMethods,
            ILanguageRepository lanRepository, IDomainRepository domainRepository, ICommentRepository commentRepository):base(accessor)
        {
            _productRepository = productRepository;
            _accessor = accessor;
            _lanRepository = lanRepository;
            _domainRepository = domainRepository;
            _userManager = userManager;
            _commentRepository = commentRepository;
            _domainName = this.DomainName;
            //_enyimMemcachedMethods = enyimMemcachedMethods;
        }

        [Route("{language}/product")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("{language}/product/{**slug}")]
        public IActionResult Details(long slug)
        {
            var isLoggedUser = HttpContext.User.Identity.IsAuthenticated;
            string userId = "";
            ViewBag.LoggedUser = isLoggedUser;
            userId = isLoggedUser ? HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value : "";
            var domainEntity = _domainRepository.FetchByName(_domainName);
            ViewBag.Providers = domainEntity.ReturnValue.DomainPaymentProviders.Select(_ => new SelectListModel() { Text = _.PspType.ToString(), Value = ((int)_.PspType).ToString() });
            var lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
            var entity = _productRepository.FetchByCode(slug, domainEntity.ReturnValue, userId);
            if (isLoggedUser)
            {
                 #region check cookiepart for loggedUser
                var userProductRateCookieName = $"{userId}_p{entity.ProductId}";
                if(HttpContext.Request.Cookies[userProductRateCookieName] != null)
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

            var lanId = _lanRepository.FetchBySymbol(lanIcon);
            ViewBag.LanIcon = lanIcon;
            ViewBag.CurCurrencyId = domainEntity.ReturnValue.DefaultCurrencyId;
            ViewBag.CurLanguageId = lanId;
            return View(entity);
        }

        [HttpPost]
        public async Task<IActionResult> RateProduct([FromQuery]string productId, [FromQuery]int score, [FromQuery]bool isNew)
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value;
            string prevRate = ""; 
            var userProductRateCookieName = $"{userId}_p{productId}";
            if (!isNew)//the user has rated before
            {
                 prevRate = HttpContext.Request.Cookies[userProductRateCookieName];
            }
            int preS = !string.IsNullOrWhiteSpace(prevRate) ? Convert.ToInt32(prevRate) : 0;
           
            var res = await _productRepository.RateProduct(productId, score,
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

