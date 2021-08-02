﻿using Arad.Portal.DataLayer.Contracts.General.Currency;
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
            PagedItems<ProductGroupViewModel> result = new PagedItems<ProductGroupViewModel>();
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;
            try
            {
                result = await _productGroupRepository.List(Request.QueryString.ToString());
                ViewBag.DefLangId = _lanRepository.GetDefaultLanguage().LanguageId;
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            }
            catch (Exception)
            {
            }
            return View(result);
        }
        public IActionResult AddEdit(string id="")
        {
            var model = new ProductGroupDTO();
           
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = _productGroupRepository.ProductGroupFetch(id);
            }
            
            var lan = _lanRepository.GetDefaultLanguage();
            ViewBag.ProductGroupList = _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId);
            ViewBag.LangId = lan.LanguageId;
           
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            ViewBag.CurrencyList = _curRepository.GetAllActiveCurrency();
            return View(model);
        }

        [HttpGet]
        public IActionResult GetProductGroupList(string id)
        {
            JsonResult result;
            List<SelectListModel> lst;
            lst = _productGroupRepository.GetAlActiveProductGroup(id).OrderBy(_=>_.Text).ToList();
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
                    var res = _curRepository.FetchCurrency(item.CurrencyId);
                    item.CurrencyName = res.ReturnValue.CurrencyName;
                    item.CurrencyPrefix = res.ReturnValue.Prefix;
                    item.CurrencySymbol = res.ReturnValue.Symbol;
                }

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
