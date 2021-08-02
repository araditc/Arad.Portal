using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ProductSpecificationGroupController : Controller
    {
        private readonly IProductSpecGroupRepository _productSpecGrpRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly ILanguageRepository _lanRepository;
        public ProductSpecificationGroupController(IProductSpecGroupRepository productSpecGroupRepository,
            IPermissionView permissionView, ILanguageRepository lanRepository)
        {
            _productSpecGrpRepository = productSpecGroupRepository;
            _permissionViewManager = permissionView;
            _lanRepository = lanRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;
            PagedItems<SpecificationGroupDTO> list = await _productSpecGrpRepository.List(Request.QueryString.ToString());
            return View(list);
        }
        public async Task<IActionResult> AddEdit(string id)
        {
            var model = new SpecificationGroupDTO();

            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _productSpecGrpRepository.GroupSpecificationFetch(id);
            }
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] SpecificationGroupDTO dto)
        {
            JsonResult result;
            var uniqueness = _productSpecGrpRepository.FetchByName(dto.GroupName);
            if (uniqueness != null)
            {
                ModelState.AddModelError("GroupName", Language.GetString("AlertAndMessage_AlreadyExists"));
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
                if (language != null)
                {
                    dto.LanguageName = language.LanguageName;
                }

                RepositoryOperationResult saveResult = await _productSpecGrpRepository.Add(dto);
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
                var dTO = await _productSpecGrpRepository.GroupSpecificationFetch(id);
                if (dTO == null)
                {
                    result = new JsonResult(new
                    {
                        Status = "error",
                        Message = Language.GetString("AlertAndMessage_EntityNotFound")
                    });
                }
                else
                {
                    dTO.IsDeleted = false;
                    dTO.ModificationReason = "restore productSpecificationGroup";
                    var res = await _productSpecGrpRepository.Update(dTO);
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
            catch(Exception ex)
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] SpecificationGroupDTO dto)
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
                var model = _productSpecGrpRepository.GroupSpecificationFetch(dto.SpecificationGroupId);
                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
            }
            var language = _lanRepository.FetchLanguage(dto.LanguageId);
            if (language != null)
            {
                dto.LanguageName = language.LanguageName;
            }
            RepositoryOperationResult saveResult = await _productSpecGrpRepository.Update(dto);

            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            RepositoryOperationResult opResult = await _productSpecGrpRepository.Delete(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }
    }
}
