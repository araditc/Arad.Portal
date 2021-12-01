using Arad.Portal.DataLayer.Contracts.General.Comment;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Comment;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
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
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILanguageRepository _lanRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(IProductRepository productRepository, IHttpContextAccessor accessor,
            UserManager<ApplicationUser> userManager,
            ILanguageRepository lanRepository, IDomainRepository domainRepository, ICommentRepository commentRepository)
        {
            _productRepository = productRepository;
            _accessor = accessor;
            _lanRepository = lanRepository;
            _domainRepository = domainRepository;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("{language}/product/{**slug}")]
        public IActionResult Details(long slug)
        {
            var domainName = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}";
            var isLoggedUser = HttpContext.User.Identity.IsAuthenticated;
            ViewBag.LoggedUser = isLoggedUser;
            
            var domainEntity = _domainRepository.FetchByName(domainName);
            var lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
            var entity = _productRepository.FetchByCode(slug, domainEntity.ReturnValue);
            #region check cookiepart for loggedUser
            if(isLoggedUser)
            {
                var userId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value;
                var userProductRateCookieName = $"{userId}_p{entity.ProductId}";
                if(HttpContext.Request.Cookies[userProductRateCookieName] != null)
                {
                    ViewBag.HasRate = true;
                }else
                {
                    ViewBag.HasRate = false;
                }
            }
           
            #endregion

            var lanId = _lanRepository.FetchBySymbol(lanIcon);
            ViewBag.CurCurrencyId = domainEntity.ReturnValue.DefaultCurrencyId;
            ViewBag.CurLanguageId = lanId;
            return View(entity);
        }

        [HttpPost]
        public async Task<IActionResult> RateProduct([FromHeader]string productId, [FromHeader]int score)
        {
            var res = await _productRepository.RateProduct(productId, score);
            if(res.Succeeded)
            {
                //set its related cookie
                return
                    Json(new { status = "Succeed", like = res.ReturnValue.LikeRate,
                        dislike = res.ReturnValue.DisikeRate, half = res.ReturnValue.halfLikeRate });
            }else
            {
                return Json(new { status = "error" });
            }

        }

       

    }
}

