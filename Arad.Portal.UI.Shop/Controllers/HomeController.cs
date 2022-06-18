using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.UI.Shop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Content;
using System.Security.Claims;
using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.UI.Shop.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductGroupRepository _productGroupRepository;
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _accessor;

        public HomeController(ILogger<HomeController> logger,
            IHttpContextAccessor accessor,
            ILanguageRepository lanRepo,
            IWebHostEnvironment env,
            IProductRepository productRepository,
            IProductGroupRepository groupRepository,
            IContentCategoryRepository contentCategoryRepository,
            IContentRepository contentRepository,
            IDomainRepository domainRepository) : base(accessor, env)
        {
            _logger = logger;
            _domainRepository = domainRepository;
            _lanRepository = lanRepo;
            _productRepository = productRepository;
            _productGroupRepository = groupRepository;
            _contentRepository = contentRepository;
            _accessor = accessor;
            _contentCategoryRepository = contentCategoryRepository;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            
            var result = _domainRepository.FetchByName(DomainName, false);
            var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
            var lanId = _lanRepository.FetchBySymbol(lanIcon);

            if (result.Succeeded )
            {
                if(!result.ReturnValue.IsMultiLinguals) //single language
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


                if(result.ReturnValue.HomePageDesign.Any(_=>_.LanguageId == lanId))
                {
                    var m = result.ReturnValue.HomePageDesign.FirstOrDefault(_ => _.LanguageId == lanId);
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

        public IActionResult Privacy()
        {
            return View();
        }


        //[Route("{language}/{**slug}")]
        //public async IActionResult GetData(string slug)
        //{
        //    var isLoggedUser = HttpContext.User.Identity.IsAuthenticated;
        //    string userId = "";

        //    var lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        //    var lanId = _lanRepository.FetchBySymbol(lanIcon);

        //    userId = isLoggedUser ? HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value : "";
        //    var domainEntity = _domainRepository.FetchByName(this.DomainName, false);
        //    bool isNumber;
        //    long codeNumber;
        //    if (long.TryParse(slug, out codeNumber))
        //    {
        //        isNumber = true;
        //        var contentDto = _contentRepository.FetchByCode(codeNumber);
        //        var commonData = _contentCategoryRepository.FetchByCode(codeNumber);
        //        var productEntity = _productRepository.FetchByCode(codeNumber, domainEntity.ReturnValue, userId);
        //        if (!string.IsNullOrWhiteSpace(productEntity.ProductId))
        //        {
        //            ViewBag.Providers = domainEntity.ReturnValue.DomainPaymentProviders.Select(_ => new SelectListModel()
        //            { Text = _.PspType.ToString(), Value = ((int)_.PspType).ToString() });

        //            if (isLoggedUser)
        //            {
        //                #region check cookiepart for loggedUser
        //                var userProductRateCookieName = $"{userId}_p{productEntity.ProductId}";
        //                if (HttpContext.Request.Cookies[userProductRateCookieName] != null)
        //                {
        //                    ViewBag.HasRateBefore = true;
        //                    ViewBag.PreRate = HttpContext.Request.Cookies[userProductRateCookieName];
        //                }
        //                else
        //                {
        //                    ViewBag.HasRateBefore = false;
        //                }
        //                #endregion
        //            }


        //            ViewBag.LanIcon = lanIcon;
        //            ViewBag.CurCurrencyId = domainEntity.ReturnValue.DefaultCurrencyId;
        //            ViewBag.CurLanguageId = lanId;
        //            return View("~/Views/Product/Details.cshtml", productEntity);

        //        }
        //        else if (!string.IsNullOrWhiteSpace(_productGroupRepository.FetchByCode(codeNumber)))
        //        {
        //            CommonViewModel model = new CommonViewModel();
        //            ViewData["CurLangId"] = lanId;
        //            var id = _productGroupRepository.FetchByCode(codeNumber);
        //            var grpSection = new GroupSection()
        //            {
        //                ProductGroupId = id,
        //                CountToSkip = 0,
        //                CountToTake = 4,
        //                DefaultLanguageId = lanId
        //            };
        //            grpSection.GroupsWithProducts = await _productGroupRepository.AllGroupIdsWhichEndInProducts(this.DomainName);
        //            model.GroupSection = grpSection;

        //            var proSection = new ProductsInGroupSection()
        //            {
        //                ProductGroupId = id,
        //                CountToSkip = 0,
        //                CountToTake = 4,
        //                DefaultLanguageId = lanId
        //            };
        //            model.ProductInGroupSection = proSection;
        //            //var model = _groupRepository.FetchByCode(slug);
        //            return View("~/Views/ProductGroup/Details.cshtml", model);
        //        }
        //        else if (!string.IsNullOrWhiteSpace(contentDto.ContentId))
        //        {
        //            return View("~/Views/Content/Details.cshtml", contentDto);
        //        }
        //        else if (!commonData.NotFound)
        //        {
        //            return View("~/Views/ContentCategory/");
        //        }
        //        //???? the implementation is not completed
        //    }







        //    var entity = _productRepository.FetchByCode(slug, domainEntity.ReturnValue, userId);
        //    if (!string.IsNullOrEmpty(entity.ProductId))
        //    {
        //        if (isLoggedUser)
        //        {
        //            #region check cookiepart for loggedUser
        //            var userProductRateCookieName = $"{userId}_p{entity.ProductId}";
        //            if (HttpContext.Request.Cookies[userProductRateCookieName] != null)
        //            {
        //                ViewBag.HasRateBefore = true;
        //                ViewBag.PreRate = HttpContext.Request.Cookies[userProductRateCookieName];
        //            }
        //            else
        //            {
        //                ViewBag.HasRateBefore = false;
        //            }
        //            #endregion
        //        }

        //        var lanId = _lanRepository.FetchBySymbol(lanIcon);
        //        ViewBag.LanIcon = lanIcon;
        //        ViewBag.CurCurrencyId = domainEntity.ReturnValue.DefaultCurrencyId;
        //        ViewBag.CurLanguageId = lanId;
        //        return View(entity);
        //    }
        //    else
        //    {
        //        return Redirect($"~/{lanIcon}/ExceptionHandler/PageNotFound");
        //    }

        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
