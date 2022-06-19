using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Contracts.Shop.ProductUnit;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Contracts.Shop.ProductSpecificationGroup;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Contracts.Shop.Promotion;
using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using SixLabors.ImageSharp.Formats;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IProductUnitRepository _unitRepository;
        private readonly IProductGroupRepository _productGroupRepository;
        private readonly IProductSpecGroupRepository _specGroupRepository;
       
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _curRepository;
        private readonly IConfiguration _configuration;
        private readonly IPromotionRepository _promotionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string imageSize = "";
        private readonly CodeGenerator _codeGenerator;
        
        public ProductController(UserManager<ApplicationUser> userManager,CodeGenerator codeGenerator,
            IProductRepository productRepository,
            ILanguageRepository languageRepository, IProductGroupRepository productGroupRepository,
            ICurrencyRepository currencyRepository, IProductUnitRepository unitRepository,
            IProductSpecGroupRepository specGroupRepository,IPromotionRepository promotionRepository,
            IWebHostEnvironment webHostEnvironment, IHttpClientFactory clientFactory,
            IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _productRepository = productRepository;
            _configuration = configuration;
            _lanRepository = languageRepository;
            _curRepository = currencyRepository;
            _productGroupRepository = productGroupRepository;
            _unitRepository = unitRepository;
            _clientFactory = clientFactory;
            _specGroupRepository = specGroupRepository;
            _httpContextAccessor = accessor;
            _userManager = userManager;
            _promotionRepository = promotionRepository;
            _webHostEnvironment = webHostEnvironment;
            _codeGenerator = codeGenerator;
            imageSize = _configuration["ProductImageSize:Size"];
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<ProductViewModel> result = new PagedItems<ProductViewModel>();
            
            try
            {
                result = await _productRepository.List(Request.QueryString.ToString());
                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var defLangId = _lanRepository.GetDefaultLanguage(currentUserId).LanguageId;
                ViewBag.DefLangId = defLangId;
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
                var contentRoot = _webHostEnvironment.ContentRootPath;
                //ViewBag.Path = contentRoot;
                var staticFileStorageURL = _configuration["LocalStaticFileStorage"];
                ViewBag.Path = staticFileStorageURL;
                var groupList = await _productGroupRepository.GetAlActiveProductGroup(defLangId, currentUserId);
                ViewBag.ProductGroupList = groupList;
               
                var unitList = _unitRepository.GetAllActiveProductUnit(defLangId);
                ViewBag.ProductUnitList = unitList;
            }
            catch (Exception)
            {
            }
            return View(result);
        }

        
        
        public async Task<IActionResult> AddEdit(string id = "")
        {
            
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDB = await _userManager.FindByIdAsync(currentUserId);
            if (userDB.IsSystemAccount)
            {
                var vendorList = await _userManager.GetUsersForClaimAsync(new Claim("AppRole", "True"));
                ViewBag.Vendors = vendorList.ToList().Select(_=>new SelectListModel()
                {
                    Text = _.Profile.FullName,
                    Value = _.Id
                });
            }
            else
            {
                ViewBag.Vendors = "-1";
            }
            ViewBag.ActivePromotionId = "-1";

            var imageRatioList = _productRepository.GetAllImageRatio();
            ViewBag.ImageRatio = imageRatioList;


            var model = new ProductOutputDTO();
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _productRepository.ProductFetch(id);
                var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                var cnt = 1;
                foreach (var item in model.MultiLingualProperties)
                {
                    item.Tag = cnt.ToString();
                    cnt += 1;
                }
                if (_productRepository.HasActiveProductPromotion(id))
                {
                    ViewBag.ActivePromotionId = model.Promotion.PromotionId;
                }
            }else
            {
                model.ProductCode = _codeGenerator.GetNewId();
            }

            var lan = _lanRepository.GetDefaultLanguage(currentUserId);
            var specGroupList = _specGroupRepository.AllActiveSpecificationGroup(lan.LanguageId);
            specGroupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.SpecificationGroupList = specGroupList;

            var groupList = await _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId, currentUserId);
            //groupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.ProductGroupList = groupList;

            var currencyList = _curRepository.GetAllActiveCurrency();
            ViewBag.CurrencyList = currencyList;
            ViewBag.DefCurrency = _curRepository.GetDefaultCurrency(currentUserId).ReturnValue.CurrencyId;

            ViewBag.LangId = lan.LanguageId;
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();

            var unitList = _unitRepository.GetAllActiveProductUnit(lan.LanguageId);
            unitList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.ProductUnitList = unitList;

            ViewBag.PromotionList = _promotionRepository.GetActivePromotionsOfCurrentUser(currentUserId, PromotionType.Product);
            
            ViewBag.PicSize = imageSize;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            Result opResult = await _productRepository.DeleteProduct(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }

        [HttpGet]
        public IActionResult CheckUrlFriendUniqueness(string id, string url)
        {
            var urlFriend = $"/product/{url}";
            var res = _productRepository.IsUniqueUrlFriend(urlFriend, id);

            return Json(res ? new { Status = "Success", Message = "url is unique" }
            : new { Status = "Error", Message = "url isnt unique" });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ProductInputDTO dto)
        {
            JsonResult result ;
            var errors = new List<AjaxValidationErrorModel>();
            if (!ModelState.IsValid)
            {
                

                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors
                        .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
               

                if (_productRepository.IsCodeUnique(dto.UniqueCode))
                {

                    foreach (var item in dto.MultiLingualProperties)
                    {
                        var lan = _lanRepository.FetchLanguage(item.LanguageId);
                        item.LanguageSymbol = lan.Symbol;
                        item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                        item.UrlFriend = $"/{lan.Symbol}{item.UrlFriend}";
                        item.ProductGroupNames = new();
                        foreach (var grp in dto.GroupIds)
                        {
                            var groupDto = _productGroupRepository.ProductGroupFetch(grp);
                            if (groupDto.MultiLingualProperties.Any(_ => _.LanguageId == item.LanguageId))
                            {
                                var name = groupDto.MultiLingualProperties.Where(_ => _.LanguageId == item.LanguageId).FirstOrDefault().Name;
                                item.ProductGroupNames.Add(name);
                            }
                        }
                    }

                    foreach (var item in dto.Prices)
                    {
                        var cur = _curRepository.FetchCurrency(item.CurrencyId);

                        item.PriceId = Guid.NewGuid().ToString();
                        item.Symbol = cur.ReturnValue.Symbol;
                        item.Prefix = cur.ReturnValue.Symbol;
                        item.SDate = DateHelper.ToEnglishDate(item.StartDate);
                        item.IsActive = true;
                    }

                    var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                    var path = "images/Products";
                    foreach (var pic in dto.Pictures)
                    {
                        var res = ImageFunctions.SaveImageModel(pic, path, localStaticFileStorageURL);
                        if (res.Key != Guid.Empty.ToString())
                        {
                            pic.ImageId = res.Key;
                            pic.Url = res.Value;
                            pic.Content = "";
                        }
                    }
                    Result saveResult = await _productRepository.Add(dto);
                    if (saveResult.Succeeded)
                    {
                        _codeGenerator.SaveToDB(dto.ProductCode);
                    }
                    result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                    : new { Status = "Error", saveResult.Message });
                }else
                {
                    errors.Add(new AjaxValidationErrorModel() { Key = "UniqueCode", ErrorMessage = Language.GetString("AlertAndMessage_DuplicateUniqueCode") });
                    result = Json(new { Status = "ModelError", ModelStateErrors = errors });
                }
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ProductInputDTO dto)
        {
            JsonResult result;
            var errors = new List<AjaxValidationErrorModel>();
            if (!ModelState.IsValid)
            {
              
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel
                    { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                if(_productRepository.IsCodeUnique(dto.UniqueCode.ToLower(), dto.ProductId))
                {
                    foreach (var item in dto.MultiLingualProperties)
                    {
                        var lan = _lanRepository.FetchLanguage(item.LanguageId);
                        item.LanguageSymbol = lan.Symbol;
                        item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                        item.ProductGroupNames = new();
                        foreach (var grp in dto.GroupIds)
                        {
                            var groupDto = _productGroupRepository.ProductGroupFetch(grp);
                            if (groupDto.MultiLingualProperties.Any(_ => _.LanguageId == item.LanguageId))
                            {
                                var name = groupDto.MultiLingualProperties.Where(_ => _.LanguageId == item.LanguageId).FirstOrDefault().Name;
                                item.ProductGroupNames.Add(name);
                            }
                        }
                    }

                    //var changeCulture = false;
                    foreach (var item in dto.Prices)
                    {
                        var cur = _curRepository.FetchCurrency(item.CurrencyId);

                        item.PriceId = Guid.NewGuid().ToString();
                        item.Symbol = cur.ReturnValue.Symbol;
                        item.Prefix = cur.ReturnValue.Symbol;
                        item.IsActive = true;
                    }
                    //if(changeCulture)
                    //{
                    //    CultureInfo.CurrentCulture = new CultureInfo("fa-IR");
                    //}
                    var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                    var path = "images/Products";
                    foreach (var pic in dto.Pictures)
                    {
                        Guid isGuidKey;
                        if (!Guid.TryParse(pic.ImageId, out isGuidKey))
                        {
                            //its insert and imageId which was int from client replace with guid
                            var res = ImageFunctions.SaveImageModel(pic, path, localStaticFileStorageURL);
                            if (res.Key != Guid.Empty.ToString())
                            {
                                pic.ImageId = res.Key;
                                pic.Url = res.Value;
                                pic.Content = "";
                            }
                            //otherwise its  is update then it has no url ;
                        }
                    }
                    Result saveResult = await _productRepository.UpdateProduct(dto);
                    result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                    : new { Status = "Error", saveResult.Message });
                }else
                {
                    errors.Add(new AjaxValidationErrorModel() { Key = "UniqueCode", ErrorMessage = Language.GetString("AlertAndMessage_DuplicateUniqueCode") });
                    result = Json(new { Status = "ModelError", ModelStateErrors = errors });
                }
                
            }
            return result;
        }

        //private async Task<string> GetToken()
        //{
        //    try
        //    {
        //        var tokenUrl = _configuration["StaticFilesPlace:TokenUrl"];
        //        var userName = _configuration["StaticFilesPlace:User"];
        //        var password = _configuration["StaticFilesPlace:Password"];
        //        var scope = _configuration["StaticFilesPlace:Scope"];

        //        var client = _clientFactory.CreateClient();
        //        var keyValues = new List<KeyValuePair<string, string>> {
        //            new KeyValuePair<string, string>("username", userName),
        //            new KeyValuePair<string, string>("password", password),
        //            new KeyValuePair<string, string>("scope", /*"ApiAccess"*/scope),
        //        };

        //        client.BaseAddress = new Uri(tokenUrl);
        //        var content = new FormUrlEncodedContent(keyValues);
        //        var response = await client.PostAsync(new Uri(tokenUrl), content).Result.Content.ReadAsStringAsync();
        //        //var response = await client.PostAsync(/*"/connect/token",*/ content).Content.ReadAsStringAsync();
        //        return response;
        //        //T
        //        //var data = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponseModel>(await response.Content.ReadAsStringAsync());
        //        //access_token = data.access_token;
        //        //Logger.WriteLogFile($"access token = {access_token}");

        //    }
        //    catch (Exception e)
        //    {
        //        return string.Empty;
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            JsonResult result;
            try
            {

                var res = await _productRepository.Restore(id);
                if (res.Succeeded)
                {
                    result = new JsonResult(new
                    {
                        Status = "success",
                        Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully")
                    });
                }
                else
                {
                    result = new JsonResult(new
                    {
                        Status = "error",
                        Message = Language.GetString("AlertAndMessage_TryLator")
                    });
                }
            }
            catch (Exception ex)
            {
                result = new JsonResult(new
                {
                    Status = "error",
                    Message = Language.GetString("AlertAndMessage_TryLator")
                });
            }
            return result;
        }

    }
}
