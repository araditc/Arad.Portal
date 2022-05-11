using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using System.Web;
using SixLabors.ImageSharp.Formats;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class ProductGroupController : Controller
    {
        private readonly IProductGroupRepository _productGroupRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _curRepository;
        private readonly IConfiguration _configuration;
        private readonly CodeGenerator _codeGenerator;
        private readonly string imageSize = "";
        public ProductGroupController(IProductGroupRepository productGroupRepository,CodeGenerator codeGenerator,
            ILanguageRepository lanRepository, ICurrencyRepository currencyRepository,
            IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _productGroupRepository = productGroupRepository;
            _lanRepository = lanRepository;
            _curRepository = currencyRepository;
            _codeGenerator = codeGenerator;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            imageSize = _configuration["ProductGroupImageSize:Size"];
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<ProductGroupViewModel> result = new PagedItems<ProductGroupViewModel>();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                result = await _productGroupRepository.List(Request.QueryString.ToString());
                ViewBag.DefLangId = _lanRepository.GetDefaultLanguage(currentUserId).LanguageId;
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            }
            catch (Exception)
            {
            }
            return View(result);
        }
        public async Task<IActionResult> AddEdit(string id = "")
        {
            var model = new ProductGroupDTO();
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = _productGroupRepository.ProductGroupFetch(id);
                
                var staticFileStorageURL = _configuration["LocalStaticFileStorage"];
                if (string.IsNullOrWhiteSpace(staticFileStorageURL))
                {
                    staticFileStorageURL = _webHostEnvironment.WebRootPath;
                }
               
                if (!string.IsNullOrWhiteSpace(model.GroupImage.Url))
                {
                    model.GroupImage.Url = model.GroupImage.Url.Replace("\\", "/");
                    IImageFormat format;
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(Path.Combine(staticFileStorageURL, model.GroupImage.Url), out format))
                    {
                        var imageEncoder = Configuration.Default.ImageFormatsManager.FindEncoder(format);
                        using (MemoryStream m = new MemoryStream())
                        {
                            image.Save(m, imageEncoder);
                            byte[] imageBytes = m.ToArray();

                            // Convert byte[] to Base64 String
                            string base64String = Convert.ToBase64String(imageBytes);
                            model.GroupImage.Content = $"data:image/png;base64, {base64String}";
                        }
                    }
                }
            }
            else
            {
                model.GroupCode = _codeGenerator.GetNewId();
            }
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var lan = _lanRepository.GetDefaultLanguage(currentUserId);
            ViewBag.ProductGroupList = await _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId, currentUserId);
            ViewBag.LangId = lan.LanguageId;
            ViewBag.PicSize = imageSize;
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            ViewBag.CurrencyList = _curRepository.GetAllActiveCurrency();
         
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductGroupList(string id)
        {
            JsonResult result;
            List<SelectListModel> lst;
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            lst = await _productGroupRepository.GetAlActiveProductGroup(id, currentUserId);
            if (lst.Count() > 0)
            {
                result = new JsonResult(new { Status = "success", Data = lst });
            }
            else
            {
                result = new JsonResult(new { Status = "error", Message = "" });
            }
            return result;

        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody]ProductGroupDTO dto)
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
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                    var cur = _curRepository.FetchCurrency(item.CurrencyId);
                    item.CurrencyName = cur.ReturnValue.CurrencyName;
                    item.CurrencyPrefix = cur.ReturnValue.Prefix;
                    item.CurrencySymbol = cur.ReturnValue.Symbol;
                }
                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var lang = _lanRepository.GetDefaultLanguage(currentUserId);

                var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                var path = "Images/ProductGroups";
                if(!string.IsNullOrWhiteSpace(dto.GroupImage.Content))
                {
                    var res = ImageFunctions.SaveImageModel(dto.GroupImage, path, localStaticFileStorageURL);
                    if (res.Key != Guid.Empty.ToString())
                    {
                        dto.GroupImage.ImageId = res.Key;
                        dto.GroupImage.Url = res.Value;
                        dto.GroupImage.Content = "";
                        dto.GroupImage.Title = dto.MultiLingualProperties.Any(_ => _.LanguageId == lang.LanguageId) ?
                            dto.MultiLingualProperties.First(_ => _.LanguageId == lang.LanguageId).Name : dto.MultiLingualProperties.First().Name;
                    }
                }
                Result saveResult = await _productGroupRepository.Add(dto);
                if(saveResult.Succeeded)
                {
                    _codeGenerator.SaveToDB(dto.GroupCode);
                }
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
            }
            return result;

        }

        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            JsonResult result;
            try
            {
                var dto = _productGroupRepository.ProductGroupFetch(id);
                if (dto == null)
                {
                    result = new JsonResult(new
                    {
                        Status = "error",
                        Message = Language.GetString("AlertAndMessage_EntityNotFound")
                    });
                }
                else
                {
                    var res = await _productGroupRepository.Restore(id);
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

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody]ProductGroupDTO dto)
        {
            JsonResult result;
            ProductGroupDTO model;
            if (!ModelState.IsValid)
            {
                var errors = new List<AjaxValidationErrorModel>();

                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }

                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                var path = "Images/ProductGroups";
                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var lan = _lanRepository.GetDefaultLanguage(currentUserId);

                Guid isGuidKey;
                if (!Guid.TryParse(dto.GroupImage.ImageId, out isGuidKey) && !string.IsNullOrWhiteSpace(dto.GroupImage.Content))
                {
                    //its insert and imageId which was int from client replace with guid
                    var res = ImageFunctions.SaveImageModel(dto.GroupImage, path, localStaticFileStorageURL);
                    if (res.Key != Guid.Empty.ToString())
                    {
                        dto.GroupImage.ImageId = res.Key;
                        dto.GroupImage.Url = res.Value;
                        dto.GroupImage.Content = "";
                        //dto.GroupImage.Title = dto.MultiLingualProperties.Any(_ => _.LanguageId == lan.LanguageId) ?
                        //   dto.MultiLingualProperties.First(_ => _.LanguageId == lan.LanguageId).Name : dto.MultiLingualProperties.First().Name;

                    }
                    //otherwise it is update then it doesnt need to change its url
                }
                else
                {
                   dto.GroupImage.Url = dto.GroupImage.Url.Replace("/", "\\");
                }
               
                model = _productGroupRepository.ProductGroupFetch(dto.ProductGroupId);
                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
            }

            foreach (var item in dto.MultiLingualProperties)
            {
                var lan = _lanRepository.FetchLanguage(item.LanguageId);
                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                item.LanguageName = lan.LanguageName;
                item.LanguageSymbol = lan.Symbol;
                var res = _curRepository.FetchCurrency(item.CurrencyId);
                item.CurrencyName = res.ReturnValue.CurrencyName;
                item.CurrencyPrefix = res.ReturnValue.Prefix;
                item.CurrencySymbol = res.ReturnValue.Symbol;
            }

            Result saveResult = await _productGroupRepository.Update(dto);

            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            Result opResult = await _productGroupRepository.Delete(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }
    }
}
