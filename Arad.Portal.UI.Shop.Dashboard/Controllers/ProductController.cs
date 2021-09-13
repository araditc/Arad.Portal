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
using System.Drawing;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Contracts.Shop.Promotion;
using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using Arad.Portal.UI.Shop.Dashboard.Helpers;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IProductUnitRepository _unitRepository;
        private readonly IProductGroupRepository _productGroupRepository;
        private readonly IProductSpecGroupRepository _specGroupRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _curRepository;
        private readonly IConfiguration _configuration;
        private readonly IPromotionRepository _promotionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string imageSize = "";
        public ProductController(UserManager<ApplicationUser> userManager,
            IProductRepository productRepository, IPermissionView permissionView,
            ILanguageRepository languageRepository, IProductGroupRepository productGroupRepository,
            ICurrencyRepository currencyRepository, IProductUnitRepository unitRepository,
            IProductSpecGroupRepository specGroupRepository,IPromotionRepository promotionRepository,
            IWebHostEnvironment webHostEnvironment, IHttpClientFactory clientFactory,
            IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _productRepository = productRepository;
            _configuration = configuration;
            _permissionViewManager = permissionView;
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
            imageSize = _configuration["ProductImageSize:Size"];
        }
        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<ProductViewModel> result = new PagedItems<ProductViewModel>();
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;
            try
            {
                result = await _productRepository.List(Request.QueryString.ToString());
                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var defLangId = _lanRepository.GetDefaultLanguage(currentUserId).LanguageId;
                ViewBag.DefLangId = defLangId;
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
                var contentRoot = _webHostEnvironment.ContentRootPath;
                ViewBag.Path = contentRoot;
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


        //[HttpPost]
        //public IActionResult  SaveProductImage(string file)
        //{
        //    JsonResult output;
        //    string webRootPath = _webHostEnvironment.WebRootPath;
        //    string contentRootPath = _webHostEnvironment.ContentRootPath;
        //    try
        //    {
        //        var path = Path.Combine(webRootPath, )
        //        if (!Directory.Exists("~/imgs/Products/temp"))
        //        {
        //            Directory.CreateDirectory("~/imgs/Products/temp");
        //        };
        //        var temporaryFileName = $"{DateTime.Now.Ticks}.jpg";
        //        var path = $"~/imgs/Products/temp/{temporaryFileName}";
        //        System.IO.File.Create(path);
        //        System.IO.File.WriteAllText(path, file);
        //        output =  Json(new { status = "Succeed", path = path });
        //    }
        //    catch (Exception)
        //    {
        //        output = Json(new { status = "Error", path = string.Empty });
        //    }
        //    return output;
        //}

        
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
            var model = new ProductOutputDTO();
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _productRepository.ProductFetch(id);
                if (_productRepository.HasActiveProductPromotion(id))
                {
                    ViewBag.ActivePromotionId = model.Promotion.PromotionId;
                }
            }

            var lan = _lanRepository.GetDefaultLanguage(currentUserId);

            var specGroupList = _specGroupRepository.AllActiveSpecificationGroup(lan.LanguageId);
            specGroupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.SpecificationGroupList = specGroupList;

            var groupList = await _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId, currentUserId);
            groupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
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
            RepositoryOperationResult opResult = await _productRepository.DeleteProduct(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ProductInputDTO dto)
        {
            JsonResult result;
            if (!ModelState.IsValid)
            {
                var errors = new List<AjaxValidationErrorModel>();

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
                
                foreach (var item in dto.MultiLingualProperties)
                {
                    var lan = _lanRepository.FetchLanguage(item.LanguageId);
                    item.LanguageSymbol = lan.Symbol;
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                }

                foreach (var item in dto.Prices)
                {
                    var cur = _curRepository.FetchCurrency(item.CurrencyId);
                   
                    item.PriceId = Guid.NewGuid().ToString();
                    item.Symbol = cur.ReturnValue.Symbol;
                    item.Prefix = cur.ReturnValue.Symbol;
                    item.SDate = DateHelper.ToEnglishDate(item.StartDate);
                }
                var staticFileStorageURL = _configuration["StaticFilesPlace:APIURL"];
                var path = "Images\\Products";
                foreach (var pic in dto.Pictures)
                {
                    var res = ImageFunctions.SaveImageModel(pic, path, staticFileStorageURL, _webHostEnvironment.WebRootPath);
                    if (res.Key != Guid.Empty.ToString())
                    {
                        pic.ImageId = res.Key;
                        pic.Url = res.Value;
                        pic.Content = "";
                    }
                }

                RepositoryOperationResult saveResult = await _productRepository.Add(dto);
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ProductInputDTO dto)
        {
            JsonResult result;
            if (!ModelState.IsValid)
            {
                var errors = new List<AjaxValidationErrorModel>();
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
                foreach (var item in dto.MultiLingualProperties)
                {
                    var lan = _lanRepository.FetchLanguage(item.LanguageId);
                    item.LanguageSymbol = lan.Symbol;
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                }

                foreach (var item in dto.Prices)
                {
                    var cur = _curRepository.FetchCurrency(item.CurrencyId);
                   
                    item.PriceId = Guid.NewGuid().ToString();
                    item.Symbol = cur.ReturnValue.Symbol;
                    item.Prefix = cur.ReturnValue.Symbol;
                    item.IsActive = true;
                }
                //foreach (var pic in dto.Pictures)
                //{
                //    Guid isGuidKey;
                //    if (!Guid.TryParse(pic.ImageId, out isGuidKey))
                //    {
                //        //its insert and imageId which was int from client replace with guid
                //        var res = SaveImageModel(pic);
                //        if (res.Key != Guid.Empty.ToString())
                //        {
                //            pic.ImageId = res.Key;
                //            pic.Url = res.Value;
                //            pic.Content = "";
                //        }
                //        //otherwise its  is update then it has no url ;
                //    }
                //    else
                //    {
                //        //its update properties exept url imageId and Content
                //        //it will be done in repository
                //    }
                //}
                RepositoryOperationResult saveResult = await _productRepository.UpdateProduct(dto);
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
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
