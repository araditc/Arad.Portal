using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.StaticFiles;
using System.Globalization;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Arad.Portal.DataLayer.Models.Shared.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Comment;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Currency;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared.DesignStructure;
using System.Threading;
using Serilog;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductSpecification;

using AutoMapper;
using Arad.Portal.Helpers.UI;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Product;
using Arad.Portal.Controllers.Base;
using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecification;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.UI;

using Language = Arad.Portal.DataLayer.Entities.General.Language.Language;

namespace Arad.Portal.Controllers.Product;

public class ProductController : BaseController
{
    private readonly IProductRepository _productRepository;
    private readonly IHttpContextAccessor _accessor;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly string _domainName;
    private readonly IProductSpecificationRepository _productSpecificationRepository;
    private readonly IRazorPartialToStringRenderer _renderPartial;
    private readonly ControllerHelper _controllerHelper;
    private readonly ILogger _logger;
    private readonly MinioHelper _minioHelper;

    public ProductController(IProductRepository productRepository, IHttpContextAccessor accessor,
                             UserManager<ApplicationUser> userManager, IConfiguration configuration, IProductGroupRepository grpRepository,
                             IWebHostEnvironment env, IUserRepository userRepository, ICurrencyRepository curRepository, IProductSpecificationRepository productSpecificationRepository,
                             IRazorPartialToStringRenderer renderPartial, ControllerHelper controllerHelper, ILogger logger, IMapper mapper, MinioHelper minioHelper,
                             ILanguageRepository lanRepository, IDomainRepository domainRepository, ICommentRepository commentRepository) : base(accessor, domainRepository, lanRepository)
    {
        _productRepository = productRepository;
        _accessor = accessor;
        _userManager = userManager;
        _domainName = DomainName;
        _configuration = configuration;
        _userRepository = userRepository;
        _productSpecificationRepository = productSpecificationRepository;
        _renderPartial = renderPartial;
        _controllerHelper = controllerHelper;
        _logger = logger;
        _minioHelper = minioHelper;
        //_enyimMemcachedMethods = enyimMemcachedMethods;
    }

    //[Route("{language?}/products")]
    public async ValueTask<IActionResult> Index()

    {
        ViewData["DomainTitle"] = DomainTitle;
        ViewData["PageTitle"] = GeneralLibrary.Utilities.UtilityLanguage.GetString("design_Products");
        Domain domainEntity = _controllerHelper.FetchDomainByName(DomainName, false).ReturnValue;
        string lanId = _controllerHelper.FetchLanguageBySymbol(CultureInfo.CurrentCulture.Name);
        ViewBag.CurLangId = lanId;
        RegionInfo ri = new RegionInfo(Thread.CurrentThread.CurrentUICulture.LCID);
        string curSymbol = ri.ISOCurrencySymbol;
        Currency currencyDto = _controllerHelper.GetCurrencyByItsPrefix(curSymbol);

        ModelOutputFilter res = await _controllerHelper.GetFilterList(lanId, domainEntity.Id);

        ViewBag.FilterModel = res;
        PagedItems<ProductOutputDto> model = await _controllerHelper.GetFilteredProduct(20, 0, currencyDto.Id, lanId, domainEntity.Id);

        if (User.Identity is { IsAuthenticated: false })
        {
            return View("Index", model);
        }

        string userId = User.GetUserId();
        List<UserFavorites> userFavoriteList = _controllerHelper.GetUserFavoriteList(userId, FavoriteType.Product);
        foreach (ProductOutputDto item in model.Items)
        {
            item.IsLikesByUserBefore = userFavoriteList.Any(f => f.EntityId == item.Id);
            #region check cookiepart for loggedUser
            string userProductRateCookieName = $"{userId}_pp{item.Id}";
            if (HttpContext.Request.Cookies[userProductRateCookieName] != null)
            {
                item.HasRateBefore = true;
                item.PreRate = HttpContext.Request.Cookies[userProductRateCookieName];
            }
            else
            {
                item.HasRateBefore = false;
            }
            #endregion
        }

        return View("Index", model);
    }

    [AllowAnonymous]
    [Route("{language?}/Shop")]
    public IActionResult Shop()
    {

        Result<Domain> result = _controllerHelper.FetchDomainByName(DomainName, false);
        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        string lanId = _controllerHelper.FetchLanguageBySymbol(lanIcon);
        ViewData["DomainTitle"] = DomainTitle;
        ViewData["PageTitle"] = UtilityLanguage.GetString("design_Products");

        if (result.Succeeded)
        {
            if (!result.ReturnValue.IsMultiLinguals) //single language
            {
                string lan = result.ReturnValue.DefaultLanguageId;
                Language lanEntity = _controllerHelper.FetchLanguage(lan);
                Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                                        CookieRequestCultureProvider.MakeCookieValue(new(lanEntity.Symbol))
                                        , new()
                                          {
                                              Expires = DateTimeOffset.Now.AddYears(1),
                                              Domain = result.ReturnValue.DomainName
                                          });
            }
            else
            {
                return View(new MainPageContentPart());
            }

            if (result.ReturnValue.ProductPageDesign.Any(c => c.LanguageId == lanId))
            {
                PageDesignContent pageDesignContent = result.ReturnValue.ProductPageDesign.FirstOrDefault(c => c.LanguageId == lanId);

                    return View(pageDesignContent.MainPageContainerPart);
                
            }
            else
            {
                return View(new MainPageContentPart());
            }
        }
        else
        {
            return View(new MainPageContentPart());
        }
    }

    [HttpPost]
    [Route("{language?}/product/Filter")]
    public async ValueTask<IActionResult> Filter([FromBody] SelectedFilter filter)
    {
        Domain domainEntity = _controllerHelper.FetchDomainByName(DomainName, false).ReturnValue;
        string lanId = _controllerHelper.FetchLanguageBySymbol(CultureInfo.CurrentCulture.Name);
        RegionInfo ri = new RegionInfo(Thread.CurrentThread.CurrentUICulture.LCID);
        string curSymbol = ri.ISOCurrencySymbol;
        Currency currencyDto = _controllerHelper.GetCurrencyByItsPrefix(curSymbol);
        NameValueCollection queryStrings = HttpUtility.ParseQueryString(Request.QueryString.ToString());
        if (string.IsNullOrWhiteSpace(queryStrings["page"]))
        {
            queryStrings.Set("page", "1");
        }

        if (string.IsNullOrWhiteSpace(queryStrings["pagesize"]))
        {
            queryStrings.Set("pagesize", "20");
        }
        int page = Convert.ToInt32(queryStrings["page"]);
        int pageSize = Convert.ToInt32(queryStrings["pagesize"]);
        int skip = (page - 1) * pageSize;
        PagedItems<ProductOutputDto> res = await _controllerHelper.GetFilteredProduct(pageSize, skip, currencyDto.Id, lanId, domainEntity.Id);
        ProductPageViewModel model = new ProductPageViewModel()
                                     {
                                         CurrentPage = res.CurrentPage,
                                         Filter = filter,
                                         ItemsCount = res.ItemsCount,
                                         PageSize = res.PageSize,
                                         QueryParams = $"?page={res.CurrentPage}&pagesize={res.PageSize}"
                                     };

        try
        {
            var jsonResult = new
                             {
                                 products = await _renderPartial.RenderToString("~/Views/Product/_ProductList.cshtml", res.Items),
                                 pagination = await _renderPartial.RenderToString("~/Views/Product/_ProductFilterPagination.cshtml", model),
                                 sorting = res.Items.Count > 0 ? await _renderPartial.RenderToString("~/Views/Product/_SortingSection.cshtml", filter) : ""
                             };
            return Json(jsonResult);
        }
        catch (Exception ex)
        {
            return Json("");
        }

    }

    [HttpGet]
    [Route("{language}/product/informme")]
    public async ValueTask<IActionResult> InformMe([FromQuery] long code, CancellationToken cancellationToken)
    {
        Task<ApplicationUser> userDb = _controllerHelper.GetCurrentUser(cancellationToken);
        string lanIcon = HttpContext.Request.Path.Value.Split("/")[1];
        if (User.Identity.IsAuthenticated)
        {
            string productId = _controllerHelper.FetchIdByCode(code);
            string userId = User.GetUserId();
            Result res = new Result();
            try
            {
                ApplicationUser userEntity = await _userRepository.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (userEntity != null)
                {
                    userEntity.Profile.InformProductList.Add(productId);
                    Result<ApplicationUser> updateResult = await _userRepository.UpdateAsync(u => u.Id == userId, m => m.Profile.InformProductList, userEntity.Profile.InformProductList, cancellationToken);
                    if (updateResult.Succeeded)
                    {
                        _logger.Information($"userId: {userDb.Id}, userName: {userDb.Result.UserName},adding product specification group with {userEntity.Id} id and {userEntity.UserName} done successfully");
                        res.Succeeded = true;
                        res.Message = UtilityLanguage.GetString("AlertAndMessage_OperationDoneSuccessfully");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"userId: {userDb.Id}, userName: {userDb.Result.UserName} error {e.Message} occured. stack trace: {nameof(ProductController)}/{nameof(InformMe)}");
            }
            return Json(new
                        {
                            status = res.Succeeded ? "Succeed" : "Error",
                            message = res.Succeeded ? UtilityLanguage.GetString("AlertAndMessage_OperationDoneSuccessfully") : UtilityLanguage.GetString("AlertAndMessage_ErrorInSaving")
                        });
        }
        else
        {
            return Redirect($"~/{lanIcon}/Account/Login?returnUrl=/{lanIcon}/product/{code}");
        }
    }

    [HttpGet]
    [Route("{language}/product/Download")]
    public async ValueTask<IActionResult> Download([FromQuery] long code, CancellationToken cancellationToken)
    {
        if (User.Identity.IsAuthenticated)
        {
            Task<ApplicationUser> userDb = _controllerHelper.GetCurrentUser(cancellationToken);
            string userId = User.GetUserId();
            Domain domainDto = _controllerHelper.FetchDomainByName(DomainName, false).ReturnValue;
            ProductOutputDto entity = _controllerHelper.FetchByCode(code.ToString(), domainDto, userId);
            if (!string.IsNullOrWhiteSpace(entity.Id))
            {
                string localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                string filePath = System.IO.Path.Combine(localStaticFileStorageURL, entity.ProductFileUrl);

                //update download count for this user
                if (entity.DownloadLimitationType == Enums.DownloadLimitationType.TimeDurationWithCnt ||
                    entity.DownloadLimitationType == Enums.DownloadLimitationType.DownloadCount)
                {
                    Result res = new Result();
                    DownloadLimitation upResult = (await _productRepository.GetListAsync(p => p.CreatorUserId == userId && p.Id == entity.Id && p.IsActive && !p.IsDeleted, cancellationToken)).MaxBy(p => p.DownloadLimitation.StartDate)?.DownloadLimitation;

                    upResult.DownloadedCount += 1;
                    Result<DataLayer.Entities.Shop.Product.Product> updateResult = await _productRepository.UpdateAsync(p => p.DownloadLimitation.Id == upResult.Id, m => m.DownloadLimitation, upResult, cancellationToken);
                    if (updateResult.Succeeded)
                    {
                        _logger.Information($"userId: {userDb.Id}, userName: {userDb.Result.UserName},downloading product with {upResult.Id} id and {upResult.ProductId} id of product done successfully");
                        res.Succeeded = true;
                    }
                }

                byte[] fileContent = await System.IO.File.ReadAllBytesAsync(filePath);
                string test = GetMimeTypeForFileExtension(filePath);
                return File(fileContent, GetMimeTypeForFileExtension(filePath), entity.ProductFileName);
            }
            else
            {
                return Json(null);
            }

        }
        else
        {
            return Json(null);
        }
    }

    [HttpGet]
    [Route("{language}/product/AddToComapareList")]
    public async ValueTask<IActionResult> AddToComapareList([FromQuery] long code)
    {
        try
        {
            List<string> compareList = [];

            string productId = _controllerHelper.FetchIdByCode(code);
            string remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString().Replace(".", "");
            Domain domainEntity = _controllerHelper.FetchDomainByName(DomainName, false).ReturnValue;
            if (HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.Id}") != null)
            {
                compareList = HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.Id}");
                if (compareList.Any())
                {
                    string firstElem = compareList[0];
                    ProductOutputDto productEntity = await _controllerHelper.ProductFetch(firstElem);
                    ProductOutputDto inputEntity = await _controllerHelper.ProductFetch(productId);
                    if (!productEntity.GroupIds.Intersect(inputEntity.GroupIds).Any())
                    {
                        return Json(new
                                    {
                                        status = "Error",
                                        message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_NoCommonProductGroups")
                                    });
                    }
                }
            }
            if (!compareList.Contains(productId) && compareList.Count <= 4)
            {
                compareList.Add(productId);
            }
            HttpContext.Session.SetComplexData($"compareList_{remoteIpAddress}_{domainEntity.Id}", compareList);

            return Json(new
                        {
                            status = "Succeed",
                            message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_AddToCompareList")
                        });
        }
        catch (Exception)
        {
            return Json(new
                        {
                            status = "Error",
                            message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_ErrorOccurrence")
                        });
        }
    }

    [HttpGet]
    [Route("{language}/product/AddToComparePage")]
    public async ValueTask<IActionResult> AddToComparePage([FromQuery] long code)
    {
        string lanIcon = "";
        List<string> compareList = new();
        Domain domainEntity = _controllerHelper.FetchDomainByName(DomainName, false).ReturnValue;
        string remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString().Replace(".", "");

        if (CultureInfo.CurrentCulture.Name != null)
        {
            lanIcon = CultureInfo.CurrentCulture.Name;
        }
        else
        {
            lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
        }

        string productId = _controllerHelper.FetchIdByCode(code);
        if (!string.IsNullOrWhiteSpace(productId))
        {
            if (HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.Id}") != null)
            {
                compareList = HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.Id}");
                if (compareList.Any())
                {
                    string firstElem = compareList[0];
                    ProductOutputDto productEntity = await _controllerHelper.ProductFetch(firstElem);
                    ProductOutputDto inputEntity = await _controllerHelper.ProductFetch(productId);
                    if (!productEntity.GroupIds.Intersect(inputEntity.GroupIds).Any())
                    {
                        return Json(new
                                    {
                                        status = "Error",
                                        message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_NoCommonProductGroups")
                                    });
                    }
                }
            }
            if (!compareList.Contains(productId) && compareList.Count <= 4)
            {
                compareList.Add(productId);
            }
            HttpContext.Session.SetComplexData($"compareList_{remoteIpAddress}_{domainEntity.Id}", compareList);
        }
        return Redirect($"/{lanIcon}/Product/Compare");
    }

    [HttpGet]
    [Route("{language}/product/DeleteProductFromCompareList")]
    public IActionResult DeleteProductFromCompareList([FromQuery] long code)
    {
        List<string> compareList = new();
        string lanIcon = "";
        if (CultureInfo.CurrentCulture.Name != null)
        {
            lanIcon = CultureInfo.CurrentCulture.Name;
        }
        else
        {
            lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
        }
        Domain domainEntity = _controllerHelper.FetchDomainByName(DomainName, false).ReturnValue;
        string remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString().Replace(".", "");
        //var userId = User.GetUserId();
        ProductOutputDto dto = _controllerHelper.FetchByCode(code.ToString(), domainEntity, "");
        if (!string.IsNullOrWhiteSpace(dto.Id))
        {
            if (HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.Id}") != null)
            {
                compareList = HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.Id}");
            }
            if (compareList.Contains(dto.Id))
            {
                compareList.Remove(dto.Id);
            }
            HttpContext.Session.SetComplexData($"compareList_{remoteIpAddress}_{domainEntity.Id}", compareList);
        }

        return RedirectToAction("Compare");
    }
    [HttpGet]
    [Route("{language}/product/compare")]
    public async ValueTask<IActionResult> Compare()
    {
        List<string> compareList = new();
        List<string> specificationIds = new();
        CompareModel model = new CompareModel();
        Domain domainEntity = _controllerHelper.FetchDomainByName(DomainName, false).ReturnValue;
        string lanId = string.Empty;



        #region currency and language
        string curId = string.Empty;
        if (CultureInfo.CurrentCulture.Name != null)
        {
            lanId = _controllerHelper.FetchLanguageBySymbol(CultureInfo.CurrentCulture.Name);
            RegionInfo ri = new RegionInfo(Thread.CurrentThread.CurrentUICulture.LCID);
            string curSymbol = ri.ISOCurrencySymbol;
            Currency currencyDto = _controllerHelper.GetCurrencyByItsPrefix(curSymbol);
            curId = currencyDto.Id;
            ViewBag.CurrencySymbol = curSymbol;
        }
        else
        {
            string lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
            lanId = _controllerHelper.FetchLanguageBySymbol(lanIcon);
            ViewBag.LanIcon = lanIcon;
            curId = domainEntity.DefaultCurrencyId;
            Result<Currency> curDto = _controllerHelper.FetchCurrency(curId);
            ViewBag.CurrencySymbol = curDto.ReturnValue.Symbol;
        }

        #endregion
        List<ProductCompare> products = new();
        List<string> firstProductGroupIds = new List<string>();
        IPAddress remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;

        if (HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.Id}") != null)
        {
            compareList = HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.Id}");
        }
        int index = 0;
        foreach (string item in compareList)
        {
            ProductOutputDto productDto = await _controllerHelper.ProductFetch(item);
            if (index == 0)
            {
                firstProductGroupIds = productDto.GroupIds;
            }
            ProductCompare obj = new ProductCompare()
                                 {
                                     ProductId = item,
                                     ProductCode = productDto.ProductCode,
                                     Specifications = productDto.Specifications.Any(v => v.LanguageId == lanId) ?
                                                          productDto.Specifications.Where(v => v.LanguageId == lanId).ToList() : new(),
                                     ProductName = productDto.MultiLingualProperties.Any(p => p.LanguageId == lanId) ?
                                                       productDto.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId).Name :
                                                       productDto.MultiLingualProperties.FirstOrDefault().Name,
                                     CurrentPrice = _controllerHelper.EvaluateFinalPrice(item, productDto.Prices, productDto.GroupIds, curId).PriceValWithPromotion,
                                     ProductImageUrl = productDto.Images.Any(i => i.IsMain) ? productDto.Images.FirstOrDefault(i => i.IsMain).Url :
                                                           productDto.Images.Any(i => i.ImageRatio == ImageRatio.Square) ?
                                                               productDto.Images.FirstOrDefault(i => i.ImageRatio == ImageRatio.Square).Url : productDto.Images.FirstOrDefault().Url
                                 };
            Image image = productDto.Images.FirstOrDefault(i => i.IsMain);
            CultureInfo current = new("en-US")
                                  {
                                      DateTimeFormat = new()
                                                       {
                                                           Calendar = new GregorianCalendar()
                                                       }
                                  };
            Thread.CurrentThread.CurrentCulture = current;

            Domain domain = _controllerHelper.GetCurrentUserDomain();
            string objectName = $"{domain.Id}/{image.ImageId}/{image.FileName.Replace(':', '-')}";
            (bool success, byte[] imageData) = await _minioHelper.GetObject("productimage", objectName);
            if (success)
            {
                image.Content = Convert.ToBase64String(imageData);
            }
            obj.ProductImageUrl = image.Content;

            products.Add(obj);
            if (specificationIds.Count == 0)
            {
                specificationIds.AddRange(obj.Specifications.Select(v => v.SpecificationId).ToList());
            }
            else
            {
                specificationIds = specificationIds.Union(obj.Specifications.Select(v => v.SpecificationId).ToList()).ToList();
            }
            index++;
        }
        model.ProductComapreList = products;
        foreach (string spec in specificationIds)
        {
            if (!string.IsNullOrWhiteSpace(spec))
            {
                string productSpecificationName = string.Empty;
                ProductSpecification entity = _productSpecificationRepository.FirstOrDefault(s => s.Id == spec);

                if (entity != null)
                {
                    productSpecificationName = entity.SpecificationNameValues.Any(p => p.LanguageId == lanId) ?
                                                   entity.SpecificationNameValues.FirstOrDefault(p => p.LanguageId == lanId).Name : "";
                }

                SelectListModel obj = new SelectListModel()
                                      {
                                          Value = spec,
                                          Text = productSpecificationName
                                      };
                model.UnionSpecifications.Add(obj);
            }
        }

        Result<List<ProductCompare>> res = new Result<List<ProductCompare>>();
        res.ReturnValue = new();

        try
        {
            // Base query for filtering products
            IEnumerable<DataLayer.Entities.Shop.Product.Product> query = _productRepository.GetAll()
                                                                                           .Where(p => p.GroupIds.Intersect(firstProductGroupIds).Any());

            // Domain-specific filtering
            if (!domainEntity.IsDefault)
            {
                query = query.Where(p => p.AssociatedDomainId == domainEntity.Id);
            }
            foreach (DataLayer.Entities.Shop.Product.Product item in query)
            {
                foreach (Image item2 in item.Images)
                {
                    CultureInfo current = new("en-US")
                                          {
                                              DateTimeFormat = new()
                                                               {
                                                                   Calendar = new GregorianCalendar()
                                                               }
                                          };
                    Thread.CurrentThread.CurrentCulture = current;

                    Domain domain = _controllerHelper.GetCurrentUserDomain();
                    string objectName = $"{domain.Id}/{item2.ImageId}/{item2.FileName.Replace(':', '-')}";
                    (bool success, byte[] imageData) = await _minioHelper.GetObject("productimage", objectName);
                    if (success)
                    {
                        item2.Content = Convert.ToBase64String(imageData);
                    }
                }
            }
            Language defLang = _controllerHelper.GetDefaultLanguage();
            CultureInfo current2 = new(defLang.Symbol)
                                   {
                                       DateTimeFormat = new()
                                                        {
                                                            Calendar = new GregorianCalendar()
                                                        }
                                   };
            Thread.CurrentThread.CurrentCulture = current2;
            // Fetch and project the filtered products
            res.ReturnValue = query.OrderByDescending(p => p.SaleCount)
                                   .Select(p => new ProductCompare
                                                {
                                                    ProductCode = p.ProductCode,
                                                    ProductId = p.Id,
                                                    ProductImageUrl = p.Images.Any(img => img.IsMain || img.ImageRatio == ImageRatio.Square) ?
                                                                          p.Images.First(img => img.IsMain || img.ImageRatio == ImageRatio.Square).Content :
                                                                          p.Images.First().Content,
                                                    ProductName = p.MultiLingualProperties.FirstOrDefault(a => a.LanguageId == lanId)?.Name ??
                                                                  p.MultiLingualProperties.First().Name,
                                                    CurrentPrice = _controllerHelper.EvaluateFinalPrice(p.Id, p.Prices, p.GroupIds, curId).PriceValWithPromotion
                                                })
                                   .Take(10).ToList();

            res.Succeeded = true;
        }
        catch (Exception ex)
        {
            res.Message = ConstMessages.InternalServerErrorMessage;
            // Optionally log the exception
        }

        model.SuggestionProducts = res.ReturnValue;
        return View(model);
    }

    //[HttpGet]
    //[Route("{language}/product/search")]
    //public async Task<IActionResult> Search(string filter)
    //{
    //    string lanId = "";
    //    string curId = "";
    //    var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
    //    List<string> compareList = new List<string>();
    //    var domainEntity = _controllerHelper.FetchDomainByName(this.DomainName, false).ReturnValue;
    //    if (CultureInfo.CurrentCulture.Name != null)
    //    {
    //        lanId = _controllerHelper.FetchLanguageBySymbol(CultureInfo.CurrentCulture.Name);
    //        var ri = new RegionInfo(System.Threading.Thread.CurrentThread.CurrentUICulture.LCID);
    //        var curSymbol = ri.ISOCurrencySymbol;
    //        var currencyDto = _controllerHelper.GetCurrencyByItsPrefix(curSymbol);
    //        curId = currencyDto.Id;
    //    }
    //    else
    //    {
    //        var lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
    //        lanId = _controllerHelper.FetchLanguageBySymbol(lanIcon);
    //        curId = domainEntity.DefaultCurrencyId;
    //    }
    //    //var res = await _controllerHelper.SearchProducts(filter, lanId, curId, domainEntity.Id);
    //    if (HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.Id}") != null)
    //    {
    //        compareList = HttpContext.Session.GetComplexData<List<string>>($"compareList_{remoteIpAddress}_{domainEntity.Id}");
    //    }
    //    List<ProductCompare> final = new List<ProductCompare>();
    //    if (res.Succeeded)
    //    {
    //        //remove current compare list from result
    //        final = res.ReturnValue.Where(_ => !compareList.Contains(_.Id)).ToList();
    //        foreach (var item in final)
    //        {

    //            item.FormattedPrice = $"{Convert.ToInt64(item.CurrentPrice):n0}";
    //        }
    //    }
    //    return new JsonResult(new { status = res.Succeeded ? "success" : "failed", data = final, msg = res.Message });

    //}

    public string GetMimeTypeForFileExtension(string filePath)
    {
        const string DEFAULT_CONTENT_TYPE = "application/octet-stream";

        FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(filePath, out string contentType))
        {
            contentType = DEFAULT_CONTENT_TYPE;
        }

        return contentType;
    }

    [HttpPost]
    [Route("{language}/product/FindProductInventory")]
    public async ValueTask<IActionResult> FindProductInventory([FromBody] InventoryModel model)
    {
        try
        {
            ProductOutputDto productEntity = await _controllerHelper.ProductFetch(model.ProductId);
            List<InventoryDetail> lst = productEntity.Inventory;
            foreach (SelectListModel item in model.SelectedSpecs)
            {
                lst = lst.Where(d => d.SpecValues.Any(a => a.SpecificationId == item.Value && a.SpecificationValue == item.Text)).ToList();
            }
            int finalInventory = lst.Sum(d => d.Count);
            return Json(new { status = "success", cnt = finalInventory });
        }
        catch (Exception ex)
        {

            return Json(new { status = "error", message = GeneralLibrary.Utilities.UtilityLanguage.GetString(ConstMessages.InternalServerErrorMessage) });
        }

    }

    [Route("{language}/product/{**slug}")]
    public async ValueTask<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        bool isLoggedUser = HttpContext.User.Identity.IsAuthenticated;
        string userId = "";
        ViewData["DomainTitle"] = DomainTitle;
        userId = isLoggedUser ? User.GetUserId() : "";
        Result<Domain> domainEntity = _controllerHelper.FetchDomainByName(_domainName, false);
        ViewBag.Providers = domainEntity.ReturnValue.DomainPaymentProviders
                                        .Select(d => new SelectListModel() { Text = d.PspType.ToString(), Value = ((int)d.PspType).ToString() });
        string lanIcon = _accessor.HttpContext.Request.Path.Value.Split("/")[1];
        string languageId;
        string cookieName = CookieRequestCultureProvider.DefaultCookieName;
        if (HttpContext.Request.Cookies[cookieName] != null)
        {
            string lanSymbol = HttpContext.Request.Cookies[cookieName];
            string defLangSymbol = lanSymbol.Split("|")[0][2..];
            languageId = _controllerHelper.FetchLanguageBySymbol(defLangSymbol);

        }
        else
        {
            Language lanEntity = _controllerHelper.FetchLanguage(domainEntity.ReturnValue.DefaultLanguageId);
            languageId = lanEntity.Id;
        }


        ProductOutputDto entity = _controllerHelper.FetchByCode(slug, domainEntity.ReturnValue, userId);

        if (!string.IsNullOrEmpty(entity.Id))
        {
            entity.TotalInventory = entity.Inventory?.Sum(d => d.Count) ?? 0;

            Result updateVisitCountResult = new Result();
            DataLayer.Entities.Shop.Product.Product productEntity = await _productRepository.FirstOrDefaultAsync(p => p.Id == entity.Id, cancellationToken);
            if (productEntity != null)
            {
                productEntity.VisitCount += 1;
                Result<DataLayer.Entities.Shop.Product.Product> updateResult = await _productRepository.UpdateAsync(productEntity, cancellationToken);
                if (updateResult.Succeeded)
                {
                    updateVisitCountResult.Succeeded = true;
                    updateVisitCountResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    updateVisitCountResult.Message = ConstMessages.GeneralError;
                }
            }
            else
            {
                updateVisitCountResult.Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
            }

            if (isLoggedUser)
            {
                #region check cookiepart for loggedUser
                string userProductRateCookieName = $"{userId}_pp{entity.Id}";
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
                ApplicationUser userEntity = await _userManager.FindByIdAsync(userId);
                List<UserFavorites> userFavoriteList = _controllerHelper.GetUserFavoriteList(userId, FavoriteType.Product);

                if (userFavoriteList != null)
                {
                    if (userFavoriteList.Any(f => f.EntityId == entity.Id))
                    {
                        ViewBag.Like = true;
                    }
                    else
                    {
                        ViewBag.Like = false;
                    }
                }
                else
                {
                    ViewBag.Like = false;
                }
                #endregion
            }

            string lanId = _controllerHelper.FetchLanguageBySymbol(lanIcon);
            ViewBag.LanIcon = lanIcon;
            string? defaultCurrencyId = domainEntity.ReturnValue.DefaultCurrencyId;
            ViewBag.CurCurrencyId = defaultCurrencyId;
            ViewBag.CurLanguageId = lanId;
            ViewBag.Price = (entity.Prices.Any(p => p.StartDate <= DateTime.Now && p.CurrencyId == defaultCurrencyId && p is { IsActive: true, EndDate: null }) ?
                                 entity.Prices.FirstOrDefault(p => p.StartDate <= DateTime.Now && p.CurrencyId == ViewBag.CurCurrencyId && p is { IsActive: true, EndDate: null }) : null)!;
            ViewData["PageTitle"] = entity.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == languageId)?.Name;

            List<string>? tagKeywords = entity.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == languageId)?.TagKeywords;

            if (tagKeywords != null)
            {
                ViewData["Tags"] = string.Join(",", tagKeywords);
            }

            ViewData["SeoTitle"] = entity.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == languageId)?.SeoTitle;
            ViewData["SeoDesc"] = entity.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == languageId)?.SeoDescription;

            foreach (Image image in entity.Images)
            {
                CultureInfo current = new("en-US")
                                      {
                                          DateTimeFormat = new()
                                                           {
                                                               Calendar = new GregorianCalendar()
                                                           }
                                      };
                Thread.CurrentThread.CurrentCulture = current;

                Domain domain = _controllerHelper.GetCurrentUserDomain();
                string objectName = $"{domain.Id}/{image.ImageId}/{image.FileName.Replace(':', '-')}";
                (bool success, byte[] imageData) = await _minioHelper.GetObject("productimage", objectName);
                if (success)
                {
                    image.Content = Convert.ToBase64String(imageData);
                }
            }
            Language defLang = _controllerHelper.GetDefaultLanguage();
            CultureInfo current2 = new(defLang.Symbol)
                                   {
                                       DateTimeFormat = new()
                                                        {
                                                            Calendar = new GregorianCalendar()
                                                        }
                                   };
            Thread.CurrentThread.CurrentCulture = current2;
            return View(entity);
        }
        else
        {
            return Redirect($"~/{lanIcon}/ExceptionHandler/PageNotFound");
        }

    }


}