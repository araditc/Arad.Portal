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
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Localization;
using Arad.Portal.DataLayer.Contracts.General.User;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.StaticFiles;
using System.Globalization;
using Arad.Portal.DataLayer.Contracts.General.Currency;

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
        private readonly IUserRepository _userRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IConfiguration _configuration;
        private readonly string _domainName;
        private readonly ICurrencyRepository _curRepository;

        public ProductController(IProductRepository productRepository, IHttpContextAccessor accessor,
            UserManager<ApplicationUser> userManager, IConfiguration configuration,
            IWebHostEnvironment env, IUserRepository userRepository,ICurrencyRepository curRepository,
            ILanguageRepository lanRepository, IDomainRepository domainRepository, ICommentRepository commentRepository):base(accessor, domainRepository)
        {
            _productRepository = productRepository;
            _accessor = accessor;
            _lanRepository = lanRepository;
            _domainRepository = domainRepository;
            _userManager = userManager;
            _commentRepository = commentRepository;
            _domainName = this.DomainName;
            _configuration = configuration;
            _userRepository = userRepository;
            _curRepository = curRepository;
            
            //_enyimMemcachedMethods = enyimMemcachedMethods;
        }

        [Route("{language?}/products")]
        public IActionResult Index()
        {
            ViewData["DomainTitle"] = this.DomainTitle;
            ViewData["PageTitle"] = Language.GetString("design_Products");
            return View();
        }

        [HttpGet]
        [Route("{language}/product/Download")]
        public async Task<IActionResult> Download([FromQuery]long code)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value;
                var domaindto = _domainRepository.FetchByName(this.DomainName, false).ReturnValue;
                var entity = _productRepository.FetchByCode(code.ToString(), domaindto, userId);
                var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                var filePath = System.IO.Path.Combine(localStaticFileStorageURL, entity.ProductFileUrl);
               
                //update download count for this user
                if (entity.DownloadLimitationType == Enums.DownloadLimitationType.TimeDurationWithCnt ||
                      entity.DownloadLimitationType == Enums.DownloadLimitationType.DownloadCount)
                {
                    await _productRepository.UpdateDownloadLimitationCount(userId, entity.ProductId);
                }
                
                byte[] fileContent = await System.IO.File.ReadAllBytesAsync(filePath);
                var test = GetMimeTypeForFileExtension(filePath);
                return File(fileContent, GetMimeTypeForFileExtension(filePath), entity.ProductFileName);
            }
            else
                return Json(null);
        }

        [HttpGet]
        [Route("{language}/product/AddToComapareList")]
        public IActionResult AddToComapareList([FromQuery] long code)
        {
            try
            {
                List<string> compareList = new List<string>();
                
                var productId = _productRepository.FetchIdByCode(code);
                var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString().Replace(".", "");
                var domainEntity = _domainRepository.FetchByName(this.DomainName, false).ReturnValue;
                if (HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.DomainId}") != null)
                {
                    compareList = HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.DomainId}");
                }
                if(!compareList.Contains(productId) && compareList.Count <= 4)
                {
                    compareList.Add(productId);
                }
                HttpContext.Session.SetComplexData($"compareList_{remoteIpAddress}_{domainEntity.DomainId}", compareList);

                return Json(new
                {
                    status = "Succeed" ,
                    message = Language.GetString("AlertAndMessage_AddToCompareList")
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    status = "Error",
                    message = Language.GetString("AlertAndMessage_ErrorOccurrence")
                });
            }
        }

        [HttpGet]
        [Route("{language}/product/compare")]
        public async Task<IActionResult> Compare()
        {
            List<string> compareList = new List<string>();
            List<string> specificationIds = new List<string>();
            var model = new CompareModel();
            var domainEntity = _domainRepository.FetchByName(this.DomainName, false).ReturnValue;
            string lanId = string.Empty;
           

            if(CultureInfo.CurrentCulture.Name != null)
            {
                 lanId = _lanRepository.FetchBySymbol(CultureInfo.CurrentCulture.Name);
            }else
            {
               var lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
                lanId = _lanRepository.FetchBySymbol(lanIcon);
            }
            #region currency
            string curId = string.Empty;
            if(CultureInfo.CurrentCulture.Name != null)
            {
                var ri = new RegionInfo(System.Threading.Thread.CurrentThread.CurrentUICulture.LCID);
                var curSymbol = ri.ISOCurrencySymbol;
                var currencyDto = _curRepository.GetCurrencyByItsPrefix(curSymbol);
                curId = currencyDto.CurrencyId;
                ViewBag.CurrencySymbol = curSymbol;
            }
            else
            {

                curId = domainEntity.DefaultCurrencyId;
                var curDto = _curRepository.FetchCurrency(curId);
                ViewBag.CurrencySymbol = curDto.ReturnValue.Symbol;
            }
            
            #endregion
            List<ProductCompare> products = new List<ProductCompare>();
            var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
           
            if (HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.DomainId}") != null)
            {
                compareList = HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.DomainId}");
            }
            foreach (var item in compareList)
            {
                var productDto = await _productRepository.ProductFetch(item);
                var obj = new ProductCompare()
                {
                    ProductId = item,
                    Specifications = productDto.Specifications.Any(_=>_.LanguageId == lanId) ? 
                        productDto.Specifications.Where(_=>_.LanguageId == lanId).ToList() : new List<ProductSpecificationValue>() ,
                    ProductName = productDto.MultiLingualProperties.Any(_=> _.LanguageId == lanId) ? 
                        productDto.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).Name :
                      productDto.MultiLingualProperties.FirstOrDefault().Name,
                    CurrentPrice = _productRepository.EvaluateFinalPrice(item, productDto.Prices, productDto.GroupIds, curId).PriceValWithPromotion,
                    ProductImageUrl = productDto.Images.Any(_=>_.IsMain) ? productDto.Images.FirstOrDefault(_ => _.IsMain).Url :
                    (productDto.Images.Any(_=>_.ImageRatio == ImageRatio.Square) ?
                      productDto.Images.FirstOrDefault(_ => _.ImageRatio == ImageRatio.Square).Url : productDto.Images.FirstOrDefault().Url)
                };
                products.Add(obj);
                if(specificationIds.Count == 0)
                {
                    specificationIds.AddRange(obj.Specifications.Select(_ => _.SpecificationId).ToList());
                }else
                {
                    specificationIds = specificationIds.Union(obj.Specifications.Select(_ => _.SpecificationId).ToList()).ToList();
                }
            }
            model.ProductComapreList = products;
            foreach (var spec in specificationIds)
            {
                if(!string.IsNullOrWhiteSpace(spec))
                {
                    var obj = new SelectListModel()
                    {
                        Value = spec,
                        Text = _productRepository.GetProductSpecificationName(spec, lanId)
                    };
                    model.UnionSpecifications.Add(obj);
                }
                
                
            }
            return View(model);
        }

        public string GetMimeTypeForFileExtension(string filePath)
        {
            const string DefaultContentType = "application/octet-stream";

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = DefaultContentType;
            }

            return contentType;
        }

        [Route("{language}/product/{**slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var isLoggedUser = HttpContext.User.Identity.IsAuthenticated;
            string userId = "";
            ViewData["DomainTitle"] = this.DomainTitle;
            userId = isLoggedUser ? HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value : "";
            var domainEntity = _domainRepository.FetchByName(_domainName, false);
            ViewBag.Providers = domainEntity.ReturnValue.DomainPaymentProviders
                .Select(_ => new SelectListModel() { Text = _.PspType.ToString(), Value = ((int)_.PspType).ToString() });
            var lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
            string languageId;
            var cookieName = CookieRequestCultureProvider.DefaultCookieName;
            if(HttpContext.Request.Cookies[cookieName] != null)
            {
                var lanSymbol = HttpContext.Request.Cookies[cookieName];
                var defLangSymbol = lanSymbol.Split("|")[0][2..];
                languageId = _lanRepository.FetchBySymbol(defLangSymbol);
                
            }else
            {
                var lanEntity = _lanRepository.FetchLanguage(domainEntity.ReturnValue.DefaultLanguageId);
                languageId = lanEntity.LanguageId;
            }
             

            var entity = _productRepository.FetchByCode(slug, domainEntity.ReturnValue, userId);
            //if (isLoggedUser && entity.ProductType == Enums.ProductType.File)
            //{
            //    ViewBag.IsDownloadable = _productRepository.IsDownloadIconShowForCurrentUser(userId, entity.ProductId);
            //}else
            //{
            //    ViewBag.IsDownloadable = false;
            //}
            if (!string.IsNullOrEmpty(entity.ProductId))
            {
                var updateVisitCount = await _productRepository.UpdateVisitCount(entity.ProductId);
                if (isLoggedUser)
                {
                    #region check cookiepart for loggedUser
                    var userProductRateCookieName = $"{userId}_p{entity.ProductId}";
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
                    var userFavoriteList = _userRepository.GetUserFavoriteList(userId, FavoriteType.Product);

                    if(userFavoriteList.Any(_=>_.EntityId == entity.ProductId))
                    {
                        ViewBag.Like = true;
                    }
                    else
                    {
                        ViewBag.Like = false;
                    }
                    #endregion
                }

                var lanId = _lanRepository.FetchBySymbol(lanIcon);
                ViewBag.LanIcon = lanIcon;
                ViewBag.CurCurrencyId = domainEntity.ReturnValue.DefaultCurrencyId;
                ViewBag.CurLanguageId = lanId;
                ViewData["PageTitle"] = entity.MultiLingualProperties.FirstOrDefault(_=>_.LanguageId == languageId).Name;
                return View(entity);
            }else
            {
                return Redirect($"~/{lanIcon}/ExceptionHandler/PageNotFound");
            }
            
        }

        //[HttpPost]
        //public async Task<IActionResult> RatingProduct([FromBody]RateProduct model)
        //{
        //    var userId = HttpContext.User.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.NameIdentifier).Value;
        //    string prevRate = ""; 
        //    var userProductRateCookieName = $"{userId}_p{model.ProductId}";
        //    if (!model.IsNew)//the user has rated before
        //    {
        //         prevRate = HttpContext.Request.Cookies[userProductRateCookieName];
        //    }
        //    int preS = !string.IsNullOrWhiteSpace(prevRate) ? Convert.ToInt32(prevRate) : 0;
           
        //    var res = await _productRepository.RateProduct(model.ProductId, model.Score,
        //            model.IsNew, preS);
        //    if (res.Succeeded)
        //    {
        //        //set its related cookie
        //        return
        //            Json(new
        //            {
        //                status = "Succeed",
        //                like = res.ReturnValue.LikeRate,
        //                dislike = res.ReturnValue.DisikeRate,
        //                half = res.ReturnValue.HalfLikeRate
        //            });
        //    }
        //    else
        //    {
        //        return Json(new { status = "error" });
        //    }
        //}
    }
}

