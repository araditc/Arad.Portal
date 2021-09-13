using Arad.Portal.DataLayer.Contracts.General.Content;
using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Content;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ContentController : Controller
    {
        
        private readonly IContentRepository _contentRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly ILanguageRepository _lanRepository;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string imageSize = "";
        public ContentController(IContentRepository contentRepository, IWebHostEnvironment webHostEnvironment,
                                    IContentCategoryRepository contentCategoryRepository, IPermissionView permissionView,
                                    ILanguageRepository languageRepository, UserManager<ApplicationUser> userManager,
                                    IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _contentRepository = contentRepository;
            _webHostEnvironment = webHostEnvironment;
            _contentCategoryRepository = contentCategoryRepository;
            _permissionViewManager = permissionView;
            _lanRepository = languageRepository;
            _userManager = userManager;
            _configuration = configuration;
            _httpContextAccessor = accessor;
            imageSize = _configuration["ContentImageSize:Size"];
        }

        public async Task<IActionResult> List()
        {
            PagedItems<ContentViewModel> result = new PagedItems<ContentViewModel>();
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;
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

        [HttpPost]
        public IActionResult UploadImage(IFormFile upload, string CKEditorFuncNum, string CKEditor, string langCode)
        {
            JsonResult result;
            if (upload.Length <= 0) return null;
            if (!upload.IsImage())
            {
                result = new JsonResult(new {uplaoded = 0, message = Language.GetString("AlertAndMessage_NotImageMessage") });
                return result;
            }

            var fileName = Guid.NewGuid().ToString()/* + Path.GetExtension(upload.FileName).ToLower()*/;

            System.Drawing.Image image = System.Drawing.Image.FromStream(upload.OpenReadStream());
            var arr = imageSize.Split(" * ");
            int width = image.Width;
            int height = image.Height;
            if ((width > int.Parse(arr[0].Trim())) || (height > int.Parse(arr[1].Trim())) || upload.Length > int.Parse(arr[0].Trim()) * int.Parse(arr[1].Trim()))
            {
                result = new JsonResult(new { uploaded = 0, message = Language.GetString("AlertAndMessage_InvalidFileSize") });
                return result;
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "/Images/Contents/CkEditor");
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = Path.Combine(path, fileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                upload.CopyTo(stream);
            }

            var url = $"{"/Images/Contents/CkEditor/"}{fileName}";
            result = new JsonResult(new {uploaded = 1, fileName = url, message = Language.GetString("AlertAndMessage_ImageSuccessfullyUploaded") });
            return Json(result);
        }

        public async Task<IActionResult> AddEdit(string id = "")
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
          

            var model = new ContentDTO();
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _contentRepository.ContentFetch(id);
            }

            var lan = _lanRepository.GetDefaultLanguage(currentUserId);

            var categoryList = await _contentCategoryRepository.AllActiveContentCategory(lan.LanguageId, currentUserId);
            categoryList = categoryList.OrderBy(_ => _.Text).ToList();
            categoryList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "" });
            ViewBag.CatList = categoryList;

            ViewBag.LangId = lan.LanguageId;
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();


            ViewBag.AllSourceType = _contentRepository.GetAllSourceType();

            ViewBag.PicSize = imageSize;
            if(string.IsNullOrWhiteSpace(model.FileLogo))
            {
                model.FileLogo = $"/imgs/NoImage.png";
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            RepositoryOperationResult opResult = await _contentRepository.Delete(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ContentDTO dto)
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

                
                var staticFileStorageURL = _configuration["StaticFilesPlace:APIURL"];
                var path = "Images\\Contents";
                foreach (var pic in dto.Images)
                {
                    var res = ImageFunctions.SaveImageModel(pic, path, staticFileStorageURL, _webHostEnvironment.WebRootPath);
                    if (res.Key != Guid.Empty.ToString())
                    {
                        pic.ImageId = res.Key;
                        pic.Url = res.Value;
                        pic.Content = "";
                    }
                }
                RepositoryOperationResult saveResult = await _contentRepository.Add(dto);
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
            }
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ContentDTO dto)
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
                RepositoryOperationResult saveResult = await _contentRepository.Update(dto);
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


