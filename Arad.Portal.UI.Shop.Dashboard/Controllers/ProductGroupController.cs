using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ProductGroupController : Controller
    {
        private readonly IProductGroupRepository _productGroupRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _curRepository;
        public ProductGroupController(IProductGroupRepository productGroupRepository,
            IPermissionView permissionView, ILanguageRepository lanRepository, ICurrencyRepository currencyRepository)
        {
            _productGroupRepository = productGroupRepository;
            _permissionViewManager = permissionView;
            _lanRepository = lanRepository;
            _curRepository = currencyRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<ProductGroupDTO> result = new PagedItems<ProductGroupDTO>();
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;
            try
            {
                result = await _productGroupRepository.List(Request.QueryString.ToString());
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            }
            catch (Exception)
            {
            }
            return View(result);
        }
        public IActionResult AddEdit(string langId = "", string id="")
        {
            var model = new ProductGroupDTO();
            if (langId != "")
            {
                var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (!string.IsNullOrWhiteSpace(id))
                {
                    model = _productGroupRepository.ProductGroupFetch(id, langId);
                }
                else
                {
                    model.MultiLingualProperty.LanguageId = langId;
                }
                ViewBag.ProductGroupList = _productGroupRepository.GetAlActiveProductGroup(langId);
            }
           
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            ViewBag.CurrencyList = _curRepository.GetAllActiveCurrency();
            return View(model);
        }

        [HttpGet]
        public IActionResult GetProductGroupList(string langId)
        {
            JsonResult result;
            List<SelectListModel> lst;
            lst = _productGroupRepository.GetAlActiveProductGroup(langId);
            if(lst.Count() > 0)
            {
                result = new JsonResult(new { Status = "success", Data = lst });
            }else
            {
                result = new JsonResult(new { Status = "error", Message = "" });
            }
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] ProductGroupDTO dto)
        {
            JsonResult result;
            if(string.IsNullOrWhiteSpace(dto.MultiLingualProperty.Name))
            {
                ModelState.AddModelError("Name", Language.GetString("AlertAndMessage_NameRequired"));
            }
            if (string.IsNullOrWhiteSpace(dto.MultiLingualProperty.LanguageId))
            {
                ModelState.AddModelError("LanguageId", Language.GetString("AlertAndMessage_FillLangId"));
            }
            if (string.IsNullOrWhiteSpace(dto.MultiLingualProperty.CurrencyId))
            {
                ModelState.AddModelError("CurrencyId", Language.GetString("AlertAndMessage_SelectCurrency"));
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
                
                var lan = _lanRepository.FetchLanguage(dto.MultiLingualProperty.LanguageId);
                dto.MultiLingualProperty.MultiLingualPropertyId = Guid.NewGuid().ToString();
                dto.MultiLingualProperty.LanguageName = lan.LanguageName;
                dto.MultiLingualProperty.LanguageSymbol = lan.Symbol;
                var res = _curRepository.FetchCurrency(dto.MultiLingualProperty.CurrencyId);
                dto.MultiLingualProperty.CurrencyName = res.ReturnValue.CurrencyName;
                dto.MultiLingualProperty.CurrencyPrefix = res.ReturnValue.Prefix;
                dto.MultiLingualProperty.CurrencySymbol = res.ReturnValue.Symbol;

                RepositoryOperationResult saveResult = await _productGroupRepository.Add(dto);
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
                var entity = _productGroupRepository.FetchWholeProductGroup(id);
                if (entity == null)
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] ProductGroupDTO dto)
        {
            JsonResult result;
            ProductGroupDTO model;
            if (string.IsNullOrWhiteSpace(dto.MultiLingualProperty.Name))
            {
                ModelState.AddModelError("Name", Language.GetString("AlertAndMessage_NameRequired"));
            }
            if (string.IsNullOrWhiteSpace(dto.MultiLingualProperty.LanguageId))
            {
                ModelState.AddModelError("LanguageId", Language.GetString("AlertAndMessage_FillLangId"));
            }
            if (string.IsNullOrWhiteSpace(dto.MultiLingualProperty.CurrencyId))
            {
                ModelState.AddModelError("CurrencyId", Language.GetString("AlertAndMessage_SelectCurrency"));
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
                model = _productGroupRepository.ProductGroupFetch(dto.ProductGroupId, dto.MultiLingualProperty.LanguageId);
                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
            }

            var lan = _lanRepository.FetchLanguage(dto.MultiLingualProperty.LanguageId);
            dto.MultiLingualProperty.LanguageName = lan.LanguageName;
            dto.MultiLingualProperty.LanguageSymbol = lan.Symbol;
            var res = _curRepository.FetchCurrency(dto.MultiLingualProperty.CurrencyId);
            dto.MultiLingualProperty.CurrencyName = res.ReturnValue.CurrencyName;
            dto.MultiLingualProperty.CurrencyPrefix = res.ReturnValue.Prefix;
            dto.MultiLingualProperty.CurrencySymbol = res.ReturnValue.Symbol;


            RepositoryOperationResult saveResult = await _productGroupRepository.Update(dto);

            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            RepositoryOperationResult opResult = await _productGroupRepository.Delete(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }
    }
}
