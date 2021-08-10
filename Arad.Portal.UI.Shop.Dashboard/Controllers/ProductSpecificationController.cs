using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Contracts.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.ProductSpecification;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ProductSpecificationController : Controller
    {
        private readonly IProductSpecificationRepository _specificationRepository;
        private readonly IProductSpecGroupRepository _groupRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly ILanguageRepository _lanRepository;
        
        public ProductSpecificationController(IProductSpecificationRepository specificationRepository,
            IProductSpecGroupRepository productSpecGroupRepository,
            IPermissionView permissionView, ILanguageRepository lanRepository)
        {
            _specificationRepository = specificationRepository;
            _permissionViewManager = permissionView;
            _groupRepository = productSpecGroupRepository;
            _lanRepository = lanRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<ProductSpecificationViewModel> result = new PagedItems<ProductSpecificationViewModel>();
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;
            try
            {
                result = await _specificationRepository.List(Request.QueryString.ToString());
                ViewBag.DefLangId = _lanRepository.GetDefaultLanguage().LanguageId;
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            }
            catch (Exception)
            {
            }
            return View(result);
        }
        public async Task<IActionResult> AddEdit(string id = "")
        {
            var model = new ProductSpecificationDTO();
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _specificationRepository.SpecificationFetch(id);
            }

            var lan = _lanRepository.GetDefaultLanguage();
            var groupList = _groupRepository.AllActiveSpecificationGroup(lan.LanguageId);
            groupList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value =""  });
            ViewBag.SpecificationGroupList = groupList;
            ViewBag.LangId = lan.LanguageId;

            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            return View(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">is stands for languageId</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetSpecificationGroupList(string id)
        {
            JsonResult result;
            List<SelectListModel> lst;
            lst = _groupRepository.AllActiveSpecificationGroup(id).OrderBy(_ => _.Text).ToList();
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
        public async Task<IActionResult> Add([FromBody] ProductSpecificationDTO dto)
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
                var group = await _groupRepository.GroupSpecificationFetch(dto.SpecificationGroupId);
                foreach (var item in dto.SpecificationNameValues)
                {
                    var lan = _lanRepository.FetchLanguage(item.LanguageId);
                    
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                    item.GroupName = group != null && group.GroupNames.First(_ => _.LanguageId == lan.LanguageId) != null ?
                        group.GroupNames.First(_ => _.LanguageId == lan.LanguageId).Name : "";
                }

                RepositoryOperationResult saveResult = await _specificationRepository.Add(dto);
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
                var dto = _specificationRepository.SpecificationFetch(id);
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
                    var res = await _specificationRepository.Restore(id);
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
        public async Task<IActionResult> Edit([FromBody] ProductSpecificationDTO dto)
        {
            JsonResult result;
            ProductSpecificationDTO model;
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
                model = await _specificationRepository.SpecificationFetch(dto.ProductSpecificationId);
                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
            }
            var group = await _groupRepository.GroupSpecificationFetch(dto.SpecificationGroupId);
            foreach (var item in dto.SpecificationNameValues)
            {
                var lan = _lanRepository.FetchLanguage(item.LanguageId);
                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                item.LanguageName = lan.LanguageName;
                item.LanguageSymbol = lan.Symbol;
                item.GroupName = group != null && group.GroupNames.First(_ => _.LanguageId == lan.LanguageId) != null ?
                       group.GroupNames.First(_ => _.LanguageId == lan.LanguageId).Name : "";

            }

            RepositoryOperationResult saveResult = await _specificationRepository.Update(dto);

            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            RepositoryOperationResult opResult = await _specificationRepository.Delete(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }
    }
}
