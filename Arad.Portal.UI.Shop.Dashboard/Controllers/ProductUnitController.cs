using Arad.Portal.DataLayer.Contracts.Shop.ProductUnit;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Contracts.General.Language;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ProductUnitController : Controller
    {
        private readonly IProductUnitRepository _unitRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly ILanguageRepository _lanRepository;
        //private readonly UserExtensions _userExtensions;
        public ProductUnitController(IProductUnitRepository unitRepository,
            IPermissionView permissionView, ILanguageRepository lanRepository)
        {
            _unitRepository = unitRepository;
            _permissionViewManager = permissionView;
            _lanRepository = lanRepository;
        }


        [HttpGet]
        public async Task<IActionResult> List()
        {
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;
            PagedItems<ProductUnitDTO> list = await _unitRepository.List(Request.QueryString.ToString());
            return View(list);
        }
        public IActionResult AddEdit(string id)
        {
            var model = new ProductUnitDTO();

            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = _unitRepository.FetchUnit(id);
            }
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm]ProductUnitDTO dto)
        {
            JsonResult result;
            var uniqueness = _unitRepository.FetchByName(dto.UnitName);
            if (uniqueness != null)
            {
                ModelState.AddModelError("ProductUnitName", Language.GetString("AlertAndMessage_AlreadyExists"));
                //result = Json(new )
            }
            if (string.IsNullOrEmpty(dto.LanguageId))
            {
                ModelState.AddModelError("LanguageId", Language.GetString("AlertAndMessage_FillLangId"));
            }
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
                var language = _lanRepository.FetchLanguage(dto.LanguageId);
                if(language != null)
                {
                    dto.LanguageName = language.LanguageName;
                }

                RepositoryOperationResult saveResult = await _unitRepository.AddProductUnit(dto);
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
                var unitDto = _unitRepository.FetchUnit(id);
                if (unitDto == null)
                {
                    result = new JsonResult(new { Status = "error", 
                        Message = Language.GetString("AlertAndMessage_EntityNotFound") });
                }
                else
                {
                    unitDto.IsDeleted = false;
                    unitDto.ModificationReason = "restore productUnit";

                    var res = await _unitRepository.EditProductUnit(unitDto);

                    if (res.Succeeded)
                    {
                        result = new JsonResult(new { Status = "success", 
                            Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully") });
                    }
                    else
                    {
                        result = new JsonResult(new { Status = "error", 
                            Message = Language.GetString("AlertAndMessage_TryLator") });
                    }
                }
            }
            catch (Exception e)
            {
                result = new JsonResult(new { Status = "error", 
                    Message = Language.GetString("AlertAndMessage_TryLator") });
            }
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm]ProductUnitDTO dto)
        {
            JsonResult result;
            if (string.IsNullOrWhiteSpace(dto.ModificationReason))
            {
                ModelState.AddModelError("ModificationReason", Language.GetString("AlertAndMessage_ModificationReason"));
            }

            if (string.IsNullOrEmpty(dto.LanguageId))
            {
                ModelState.AddModelError("LanguageId", Language.GetString("AlertAndMessage_FillLangId"));
            }

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
                var model = _unitRepository.FetchUnit(dto.ProductUnitId);
                if(model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
            }
            var language = _lanRepository.FetchLanguage(dto.LanguageId);
            if (language != null)
            {
                dto.LanguageName = language.LanguageName;
            }
            RepositoryOperationResult saveResult = await _unitRepository.EditProductUnit(dto);
           
            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message } 
            : new { Status = "Error", saveResult.Message });
            return result;
        }

       [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            RepositoryOperationResult opResult = await _unitRepository.Delete(id);
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }
    }
}
