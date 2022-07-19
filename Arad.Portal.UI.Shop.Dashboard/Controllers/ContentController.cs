using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Content;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class ContentController : Controller
    {
        
        private readonly IContentRepository _contentRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string imageSize = "";
        private readonly CodeGenerator _codeGenerator;
        public ContentController(IContentRepository contentRepository, IWebHostEnvironment webHostEnvironment,
                                    IContentCategoryRepository contentCategoryRepository,
                                    ILanguageRepository languageRepository, UserManager<ApplicationUser> userManager,
                                    CodeGenerator codeGenerator,
                                    IDomainRepository domainRepository,
                                    IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _contentRepository = contentRepository;
            _webHostEnvironment = webHostEnvironment;
            _contentCategoryRepository = contentCategoryRepository;
            _lanRepository = languageRepository;
            _userManager = userManager;
            _configuration = configuration;
            _domainRepository = domainRepository;
            _httpContextAccessor = accessor;
            imageSize = _configuration["ContentImageSize:Size"];
            _codeGenerator = codeGenerator;
        }

        [HttpGet]
        public IActionResult CheckUrlFriendUniqueness(string id, string url)
        {
            var urlFriend = $"/blog/{url}";
            var domainId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type.Equals("RelatedDomain"))?.Value;
            if (domainId == null)
            {
                domainId = _domainRepository.FetchDefaultDomain().ReturnValue.DomainId;
            }
            var res = _contentRepository.IsUniqueUrlFriend(urlFriend, domainId, id);

            return Json(res ? new { Status = "Success", Message = "url is unique" }
            : new { Status = "Error", Message = "url isnt unique" });
        }

        public async Task<IActionResult> List()
        {
            PagedItems<ContentViewModel> result = new PagedItems<ContentViewModel>();
           
            try
            {
                result = await _contentRepository.List(Request.QueryString.ToString());

                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var defLangId = _lanRepository.GetDefaultLanguage(currentUserId).LanguageId;
                ViewBag.DefLangId = defLangId;
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();

                var categoryList =await _contentCategoryRepository.AllActiveContentCategory(defLangId, currentUserId);
                categoryList = categoryList.OrderBy(_ => _.Text).ToList();
                categoryList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
                ViewBag.CatList = categoryList;

            }
            catch (Exception ex)
            {
            }
            return View(result);
        }

       
        public async Task<IActionResult> AddEdit(string id = "")
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var staticFileStorageURL = _configuration["LocalStaticFileStorage"];

            var fileShown = _configuration["LocalStaticFileShown"];
            ViewBag.Url = fileShown;
            var model = new ContentDTO();
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _contentRepository.ContentFetch(id);
                
                if (string.IsNullOrWhiteSpace(staticFileStorageURL))
                {
                    staticFileStorageURL = _webHostEnvironment.WebRootPath;
                }
                
            }else
            {
                model.ContentCode = _codeGenerator.GetNewId();
            }

            ViewBag.StaticFileStorage = staticFileStorageURL;
            var lan = _lanRepository.GetDefaultLanguage(currentUserId);
            var categoryList = await _contentCategoryRepository.AllActiveContentCategory(lan.LanguageId, currentUserId);
            categoryList = categoryList.OrderBy(_ => _.Text).ToList();
            //categoryList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.CatList = categoryList;

            var imageRatioList = _contentRepository.GetAllImageRatio();
            ViewBag.ImageRatio = imageRatioList;

            ViewBag.LangId = lan.LanguageId;
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();


            ViewBag.AllSourceType = _contentRepository.GetAllSourceType();

            ViewBag.PicSize = imageSize;
            //if(string.IsNullOrWhiteSpace(model.FileLogo))
            //{
            //    model.FileLogo = "/imgs/NoImage.png";
            //}
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            Result opResult = await _contentRepository.Delete(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ContentDTO dto)
        {
            JsonResult result;
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
                var isCkEditorContentValid = HtmlSanitizer.SanitizeHtml(dto.Contents);
                if(!isCkEditorContentValid)
                {
                    errors.Add(new AjaxValidationErrorModel() {Key = "Contents", ErrorMessage = Language.GetString("AlertAndMessage_InvalidEditorContent") });
                    result = Json(new { Status = "ModelError", ModelStateErrors = errors });
                }
                else
                {
                    dto.UrlFriend = $"/blog/{dto.UrlFriend}";

                    var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                    var path = "images/Contents";
                    foreach (var pic in dto.Images)
                    {
                        var res = ImageFunctions.SaveImageModel(pic, path, localStaticFileStorageURL);
                        if (res.Key != Guid.Empty.ToString())
                        {
                            pic.ImageId = res.Key;
                            pic.Url = res.Value;
                            pic.Content = "";
                        }
                    }
                    //if (dto.LogoContent != "")
                    //    dto.FileLogo = dto.LogoContent;
                    
                    Result saveResult = await _contentRepository.Add(dto);
                    if (saveResult.Succeeded)
                    {
                        _codeGenerator.SaveToDB(dto.ContentCode);
                    }
                    result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                    : new { Status = "Error", saveResult.Message });
                }
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ContentDTO dto)
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
                //if (dto.LogoContent != "")
                //dto.FileLogo = dto.LogoContent;
                var isCkEditorContentValid = HtmlSanitizer.SanitizeHtml(dto.Contents);
                if (!isCkEditorContentValid)
                {
                    errors.Add(new AjaxValidationErrorModel() { Key = "Contents", ErrorMessage = Language.GetString("AlertAndMessage_InvalidEditorContent") });
                    result = Json(new { Status = "ModelError", ModelStateErrors = errors });
                }
                else
                {
                    dto.UrlFriend = $"/blog/{dto.UrlFriend}";
                    var localStaticFileStorageURL = _configuration["LocalStaticFileStorage"];
                    var path = "images/Contents";
                    foreach (var pic in dto.Images)
                    {
                        if (!string.IsNullOrEmpty(pic.Content))
                        {
                            var res = ImageFunctions.SaveImageModel(pic, path, localStaticFileStorageURL);
                            if (res.Key != Guid.Empty.ToString())
                            {
                                pic.ImageId = res.Key;
                                pic.Url = res.Value;
                                pic.Content = "";
                            }
                        }
                    }

                    Result saveResult = await _contentRepository.Update(dto);
                    result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                    : new { Status = "Error", saveResult.Message });
                }
            }
            return result;
        }

        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            JsonResult result;
            try
            {
                var res = await _contentRepository.Restore(id);
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


