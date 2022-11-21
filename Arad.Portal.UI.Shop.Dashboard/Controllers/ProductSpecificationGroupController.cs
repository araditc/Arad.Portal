using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.General.Domain.Mongo;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class ProductSpecificationGroupController : Controller
    {
        private readonly IProductSpecGroupRepository _productSpecGrpRepository;
        
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDomainRepository _domainRepository;
        public ProductSpecificationGroupController(IProductSpecGroupRepository productSpecGroupRepository,
            ILanguageRepository lanRepository, ICurrencyRepository currencyRepository,
            IDomainRepository domainrepository, UserManager<ApplicationUser> userManager)
        {
            _productSpecGrpRepository = productSpecGroupRepository;
            _lanRepository = lanRepository;
            _currencyRepository = currencyRepository;
            _userManager = userManager;
            _domainRepository = domainrepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<SpecificationGroupViewModel> list = new PagedItems<SpecificationGroupViewModel>();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDb = await _userManager.FindByIdAsync(currentUserId);
            try
            {
                list = await _productSpecGrpRepository.List(Request.QueryString.ToString(), userDb);
                ViewBag.DefLangId = _lanRepository.GetDefaultLanguage(currentUserId).LanguageId;
                var langs = _lanRepository.GetAllActiveLanguage();
                langs.Insert(0, new SelectListModel() { Text = Language.GetString("Choose"), Value = "-1" });
                ViewBag.LangList = langs;
            }
            catch (Exception)
            {
            }
            return View(list);
        }
        public async Task<IActionResult> AddEdit(string id)
        {
            var model = new SpecificationGroupDTO();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userDB = await _userManager.FindByIdAsync(currentUserId);
            if (userDB.IsSystemAccount)
            {
                ViewBag.Domains = _domainRepository.GetAllActiveDomains();
            }else
            {
                model.AssociatedDomainId = userDB.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;
            }
            ViewBag.IsSysAcc = userDB.IsSystemAccount;

            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _productSpecGrpRepository.GroupSpecificationFetch(id);
            }
            var lan = _lanRepository.GetDefaultLanguage(currentUserId);
            ViewBag.LangId = lan.LanguageId;
           
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            ViewBag.CurrencyList = _currencyRepository.GetAllActiveCurrency();
            return View(model);
        }

        [HttpPost]
       
        public async Task<IActionResult> Add([FromBody] SpecificationGroupDTO dto)
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
                foreach (var item in dto.GroupNames)
                {
                    var lan = _lanRepository.FetchLanguage(item.LanguageId);
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                    var res = _currencyRepository.FetchCurrency(item.CurrencyId);
                    item.CurrencyName = res.ReturnValue.CurrencyName;
                    item.CurrencyPrefix = res.ReturnValue.Prefix;
                    item.CurrencySymbol = res.ReturnValue.Symbol;
                }
                Result saveResult = await _productSpecGrpRepository.Add(dto);
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
                   
                var res = await _productSpecGrpRepository.Restore(id);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">id stands for languageId</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProductSpecificationGroupList(string lid, string did)
        {
            JsonResult result;
            List<SelectListModel> lst;
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            lst = (await _productSpecGrpRepository.AllActiveSpecificationGroup(lid, currentUserId, did )).OrderBy(_ => _.Text).ToList();
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
        public async Task<IActionResult> Edit([FromBody] SpecificationGroupDTO dto)
        {
            JsonResult result;
            SpecificationGroupDTO model;
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
                 model =await _productSpecGrpRepository.GroupSpecificationFetch(dto.SpecificationGroupId);
                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
            }

            foreach (var item in dto.GroupNames)
            {
                var lan = _lanRepository.FetchLanguage(item.LanguageId);
                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                item.LanguageName = lan.LanguageName;
                item.LanguageSymbol = lan.Symbol;
                var res = _currencyRepository.FetchCurrency(item.CurrencyId);
                item.CurrencyName = res.ReturnValue.CurrencyName;
                item.CurrencyPrefix = res.ReturnValue.Prefix;
                item.CurrencySymbol = res.ReturnValue.Symbol;
            }
            Result saveResult = await _productSpecGrpRepository.Update(dto);

            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }


        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            Result opResult = await _productSpecGrpRepository.Delete(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }
    }
}
