using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Models.ContentCategory;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ContentCategoryController : Controller
    {
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly ILanguageRepository _lanRepository;
        private readonly CodeGenerator _codeGenerator;
       
        public ContentCategoryController(IContentCategoryRepository contentCategoryRepository,
            CodeGenerator codeGenerator,
            IPermissionView permissionView, ILanguageRepository lanRepository)
        {
            _contentCategoryRepository = contentCategoryRepository;
            _permissionViewManager = permissionView;
            _lanRepository = lanRepository;
            _codeGenerator = codeGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<ContentCategoryViewModel> result = new PagedItems<ContentCategoryViewModel>();
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Permissions = dicKey;
            try
            {
                result = await _contentCategoryRepository.List(Request.QueryString.ToString());
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
            var model = new ContentCategoryDTO();
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _contentCategoryRepository.ContentCategoryFetch(id);
            }else
            {
                model.CategoryCode = _codeGenerator.GetNewId();
            }
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var lan = _lanRepository.GetDefaultLanguage(currentUserId);
            ViewBag.LangId = lan.LanguageId;
            var categoryList = await _contentCategoryRepository.AllActiveContentCategory(lan.LanguageId, currentUserId);
            categoryList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
            ViewBag.CategoryList = categoryList;
             var lst =  _contentCategoryRepository.GetAllContentCategoryType();
            //lst.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
            ViewBag.CategoryTypes = lst;
          

            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();          
            return View(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">id stands for langId</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetContentCategoryList(string id)
        {
            JsonResult result;
            List<SelectListModel> lst;
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            lst = await _contentCategoryRepository.AllActiveContentCategory(id, currentUserId);
            if (lst.Count() > 0)
            {
                result = new JsonResult(new { Status = "success", Data = lst.OrderBy(_=>_.Text) });
            }
            else
            {
                result = new JsonResult(new { Status = "error", Message = "" });
            }
            return result;

        }

       


        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ContentCategoryDTO dto)
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
                foreach (var item in dto.CategoryNames)
                {
                    var lan = _lanRepository.FetchLanguage(item.LanguageId);
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                }

                RepositoryOperationResult saveResult = await _contentCategoryRepository.Add(dto);
                if(saveResult.Succeeded)
                {
                    _codeGenerator.SaveToDB(dto.CategoryCode);
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
                var dto = _contentCategoryRepository.ContentCategoryFetch(id);
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
                    var res = await _contentCategoryRepository.Restore(id);
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
        public async Task<IActionResult> Edit([FromBody] ContentCategoryDTO dto)
        {
            JsonResult result;
            ContentCategoryDTO model;
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
                model =await _contentCategoryRepository.ContentCategoryFetch(dto.ContentCategoryId);
                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
            }

            foreach (var item in dto.CategoryNames)
            {
                var lan = _lanRepository.FetchLanguage(item.LanguageId);
                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                item.LanguageName = lan.LanguageName;
                item.LanguageSymbol = lan.Symbol;
            }

            RepositoryOperationResult saveResult = await _contentCategoryRepository.Update(dto);

            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            RepositoryOperationResult opResult = await _contentCategoryRepository.Delete(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }
    }
}
