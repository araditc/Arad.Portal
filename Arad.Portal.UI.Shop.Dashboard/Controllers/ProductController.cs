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
        private readonly string imageSize = "";
        public ProductController(UserManager<ApplicationUser> userManager,
            IProductRepository productRepository, IPermissionView permissionView,
            ILanguageRepository languageRepository, IProductGroupRepository productGroupRepository,
            ICurrencyRepository currencyRepository, IProductUnitRepository unitRepository,
            IProductSpecGroupRepository specGroupRepository,IPromotionRepository promotionRepository,
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _productRepository = productRepository;
            _configuration = configuration;
            _permissionViewManager = permissionView;
            _lanRepository = languageRepository;
            _curRepository = currencyRepository;
            _productGroupRepository = productGroupRepository;
            _unitRepository = unitRepository;
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
               
                var groupList = _productGroupRepository.GetAlActiveProductGroup(defLangId);
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

        public string ResizeImage(string filePath, int desiredHeight)
        {
            var base64String = System.IO.File.ReadAllText(filePath);
            byte[] byteArray = Convert.FromBase64String(base64String);
            System.Drawing.Image img;
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                img = System.Drawing.Image.FromStream(ms);
            }

            double ratio = (double)desiredHeight / img.Height;
            int newWidth = (int)(img.Width * ratio);
            int newHeight = (int)(img.Height * ratio);
            Bitmap bitMapImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(bitMapImage))
            {
                g.DrawImage(img, 0, 0, newWidth, newHeight);
            }
            img.Dispose();
            //return newImage;

            byte[] byteImage;
            using (MemoryStream ms = new MemoryStream())
            {
                bitMapImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byteImage = ms.ToArray();
            }
            return Convert.ToBase64String(byteImage);
        }
        public async Task<IActionResult> AddEdit(string id = "")
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDB = await _userManager.FindByIdAsync(currentUserId);
            if(userDB.IsSystemAccount)
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

            var groupList = _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId);
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
                    //??? just one valid record
                    item.PriceId = Guid.NewGuid().ToString();
                    item.Symbol = cur.ReturnValue.Symbol;
                    item.Prefix = cur.ReturnValue.Symbol;
                    item.IsActive = true;
                }
                foreach (var pic in dto.Pictures)
                {
                    var res = SaveImageModel(pic);
                    if(res.Key != Guid.Empty.ToString())
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

        private KeyValuePair<string, string> SaveImageModel(DataLayer.Models.Shared.Image picture)
        {
            KeyValuePair<string, string> res;
            var staticFileStorageURL = _configuration["StaticFilesPlace:Url"];
            picture.ImageId = Guid.NewGuid().ToString();
            picture.Url = $"{staticFileStorageURL}/Images/Products/{picture.ImageId}.jpg";
            var path = $"{staticFileStorageURL}/Images/Products";
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                byte[] bytes = Convert.FromBase64String(picture.Content);
                System.Drawing.Image image;
                using MemoryStream ms = new MemoryStream(bytes);
                image = System.Drawing.Image.FromStream(ms);
                image.Save(picture.Url, System.Drawing.Imaging.ImageFormat.Jpeg);
                res = new KeyValuePair<string, string>(picture.ImageId, picture.Url);
            }
            catch (Exception)
            {
                res = new KeyValuePair<string, string>(Guid.Empty.ToString(), "");
            }
            return res;
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ProductInputDTO dto)
        {
            JsonResult result;
            ProductInputDTO model;
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
                    //??? just one valid record
                    item.PriceId = Guid.NewGuid().ToString();
                    item.Symbol = cur.ReturnValue.Symbol;
                    item.Prefix = cur.ReturnValue.Symbol;
                    item.IsActive = true;
                }
                foreach (var pic in dto.Pictures)
                {
                    Guid isGuidKey;
                    if (!Guid.TryParse(pic.ImageId, out isGuidKey))
                    {
                        //its insert 
                        var res = SaveImageModel(pic);
                        if (res.Key != Guid.Empty.ToString())
                        {
                            pic.ImageId = res.Key;
                            pic.Url = res.Value;
                            pic.Content = "";
                        }
                        //otherwise its  is update then it has no url ;
                    }

                }
            }
            

            RepositoryOperationResult saveResult = await _productRepository.UpdateProduct(dto);
            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }

        //private static async Task RelayViaHttpGetAsync(string host, string query, string messageId,
        //    ApiDeliveryQueueDto model, int tryCount, string auth)
        //{

        //    FullLogOption.OptionalLog("In Relay via get.");
        //    FullLogOption.OptionalLog($"{host} {query} {JsonConvert.SerializeObject(model)}");

        //    FullLogOption.OptionalLog($"relay via get {host} {query} {JsonConvert.SerializeObject(model)}");

        //    var st = new Stopwatch();
        //    st.Start();

        //    using var serviceScope = _hostBuild.Services.CreateScope();
        //    {
        //        var services = serviceScope.ServiceProvider;
        //        _clientFactory = services.GetService<IHttpClientFactory>();
        //        var client = _clientFactory.CreateClient();

        //        if (!string.IsNullOrWhiteSpace(auth))
        //        {
        //            client.DefaultRequestHeaders.Add("Authorization", "Basic " + auth);
        //        }

        //        //client.Timeout = TimeSpan.FromMilliseconds(300);
        //        var address = Flurl.Url.Combine(host, query);

        //        FullLogOption.OptionalLog($"address:{address}");
        //        var response = await client.GetAsync(address);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            throw new ApiDeliveryEngineRetryException { CurrentRetryCount = tryCount, Model = model, TimeElapsed = st.Elapsed, Method = "Get", StatusCode = response.StatusCode };
        //        }

        //        Logger.WriteLogFile($"Get time: {st.ElapsedMilliseconds}");
        //    }
        //}

        //private static async Task RelayViaHttpPostAsync(ApiDeliveryQueueDto model, string host,
        //   List<RelayParams> additionalParams, int tryCount, string auth)
        //{
        //    FullLogOption.OptionalLog($"relay via post {host} {JsonConvert.SerializeObject(additionalParams)} {JsonConvert.SerializeObject(model)}");

        //    var st = new Stopwatch();
        //    st.Start();

        //    using var serviceScope = _hostBuild.Services.CreateScope();
        //    {
        //        var services = serviceScope.ServiceProvider;
        //        _clientFactory = services.GetService<IHttpClientFactory>();

        //        var client = _clientFactory.CreateClient();
        //        if (!string.IsNullOrWhiteSpace(auth))
        //        {
        //            client.DefaultRequestHeaders.Add("Authorization", "Basic " + auth);
        //        }

        //        var content = new StringContent(
        //            JsonConvert.SerializeObject(new
        //            {
        //                BatchId = model.Object.MessageId,
        //                model.Object.Status,
        //                model.Object.PartNumber,
        //                model.Object.Mobile,
        //                ExtraParameters = additionalParams
        //            }),
        //            Encoding.UTF8, MediaTypeNames.Application.Json);
        //        var response = await client.PostAsync(host, content);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            throw new ApiDeliveryEngineRetryException { CurrentRetryCount = tryCount, Model = model, Address = host, TimeElapsed = st.Elapsed, Method = "Post", StatusCode = response.StatusCode };
        //        }

        //        Logger.WriteLogFile($"Post time: {st.ElapsedMilliseconds}");
        //    }
        //}


        //private async string GetToken()
        //{
        //    try
        //    {
        //        Logger.WriteLogFile("Start Getting token");
        //        var client = clientFactory.CreateClient();
        //        var keyValues = new List<KeyValuePair<string, string>> {
        //            new KeyValuePair<string, string>("username", userName),
        //            new KeyValuePair<string, string>("password", password),
        //            new KeyValuePair<string, string>("scope", "ApiAccess"),
        //        };

        //        client.BaseAddress = new Uri(authEndPointBaseAddress);
        //        var content = new FormUrlEncodedContent(keyValues);
        //        var response = await client.PostAsync(/*"/connect/token",*/ content).Content.ReadAsStringAsync();
        //        return response;


        //                                                                 //T
        //        //var data = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponseModel>(await response.Content.ReadAsStringAsync());
        //        //access_token = data.access_token;
        //        //Logger.WriteLogFile($"access token = {access_token}");

        //    }
        //    catch (Exception e)
        //    {
        //        Logger.WriteLogFile(e.Message);
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
