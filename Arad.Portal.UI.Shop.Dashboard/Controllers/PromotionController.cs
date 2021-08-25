using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.Shop.Promotion;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Arad.Portal.DataLayer.Models.Promotion;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Entities.General.User;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class PromotionController : Controller
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public PromotionController(IPromotionRepository promotionRepository,
            IPermissionView permissionView, ILanguageRepository lanRepository, 
            ICurrencyRepository currencyRepository, UserManager<ApplicationUser> userManager)
        {
            _promotionRepository = promotionRepository;
            _permissionViewManager = permissionView;
            _lanRepository = lanRepository;
            _currencyRepository = currencyRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<PromotionDTO> list = new PagedItems<PromotionDTO>();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;
            try
            {
                var q = string.Empty;
                if(!string.IsNullOrWhiteSpace(Request.QueryString.ToString()))
                {
                    q = Request.QueryString.ToString() + "&"; 
                }else
                {
                    q = "?"; 
                }
                q += currentUser.IsSystemAccount ? $"userId={Guid.Empty}" : $"userId={currentUserId}";
                list = await _promotionRepository.ListPromotions(q);
                ViewBag.DefCurrencyId = _currencyRepository.GetDefaultCurrency(currentUserId);
                ViewBag.CurrencyList = _currencyRepository.GetAllActiveCurrency();
            }
            catch (Exception)
            {
            }
            return View(list);
        }
        public  IActionResult AddEdit(string id)
        {
            var model = new PromotionDTO();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = _promotionRepository.FetchPromotion(id);
            }

            ViewBag.DefCurrencyId = _currencyRepository.GetDefaultCurrency(currentUserId);
            ViewBag.CurrencyList = _currencyRepository.GetAllActiveCurrency();
            ViewBag.DiscountTypeList = _promotionRepository.GetAllDiscountType();
            ViewBag.PromotionTypeList = _promotionRepository.GetAllPromotionType();
            return View(model);
        }

        //[HttpPost]

        //public async Task<IActionResult> Add([FromBody] SpecificationGroupDTO dto)
        //{
        //    JsonResult result;
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = new List<AjaxValidationErrorModel>();

        //        foreach (var modelStateKey in ModelState.Keys)
        //        {
        //            var modelStateVal = ModelState[modelStateKey];
        //            errors.AddRange(modelStateVal.Errors
        //                .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
        //        }
        //        result = Json(new { Status = "ModelError", ModelStateErrors = errors });
        //    }
        //    else
        //    {
        //        foreach (var item in dto.GroupNames)
        //        {
        //            var lan = _lanRepository.FetchLanguage(item.LanguageId);
        //            item.MultiLingualPropertyId = Guid.NewGuid().ToString();
        //            item.LanguageName = lan.LanguageName;
        //            item.LanguageSymbol = lan.Symbol;
        //            var res = _currencyRepository.FetchCurrency(item.CurrencyId);
        //            item.CurrencyName = res.ReturnValue.CurrencyName;
        //            item.CurrencyPrefix = res.ReturnValue.Prefix;
        //            item.CurrencySymbol = res.ReturnValue.Symbol;
        //        }
        //        RepositoryOperationResult saveResult = await _productSpecGrpRepository.Add(dto);
        //        result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
        //        : new { Status = "Error", saveResult.Message });
        //    }

        //    return result;

        //}

        //[HttpGet]
        //public async Task<IActionResult> Restore(string id)
        //{
        //    JsonResult result;
        //    try
        //    {

        //        var res = await _productSpecGrpRepository.Restore(id);
        //        if (res.Succeeded)
        //        {
        //            result = new JsonResult(new
        //            {
        //                Status = "success",
        //                Message = Language.GetString("AlertAndMessage_EditionDoneSuccessfully")
        //            });
        //        }
        //        else
        //        {
        //            result = new JsonResult(new
        //            {
        //                Status = "error",
        //                Message = Language.GetString("AlertAndMessage_TryLator")
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = new JsonResult(new
        //        {
        //            Status = "error",
        //            Message = Language.GetString("AlertAndMessage_TryLator")
        //        });
        //    }
        //    return result;
        //}
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="id">id stands for languageId</param>
        ///// <returns></returns>
        //[HttpGet]
        //public IActionResult GetProductSpecificationGroupList(string id)
        //{
        //    JsonResult result;
        //    List<SelectListModel> lst;
        //    lst = _productSpecGrpRepository.AllActiveSpecificationGroup(id).OrderBy(_ => _.Text).ToList();
        //    if (lst.Count() > 0)
        //    {
        //        result = new JsonResult(new { Status = "success", Data = lst });
        //    }
        //    else
        //    {
        //        result = new JsonResult(new { Status = "error", Message = "" });
        //    }
        //    return result;

        //}

        //[HttpPost]
        //public async Task<IActionResult> Edit([FromBody] SpecificationGroupDTO dto)
        //{
        //    JsonResult result;
        //    SpecificationGroupDTO model;
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = new List<AjaxValidationErrorModel>();

        //        foreach (var modelStateKey in ModelState.Keys)
        //        {
        //            var modelStateVal = ModelState[modelStateKey];
        //            errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
        //        }

        //        result = Json(new { Status = "ModelError", ModelStateErrors = errors });
        //    }
        //    else
        //    {
        //        model = await _productSpecGrpRepository.GroupSpecificationFetch(dto.SpecificationGroupId);
        //        if (model == null)
        //        {
        //            return RedirectToAction("PageOrItemNotFound", "Account");
        //        }
        //    }

        //    foreach (var item in dto.GroupNames)
        //    {
        //        var lan = _lanRepository.FetchLanguage(item.LanguageId);
        //        item.MultiLingualPropertyId = Guid.NewGuid().ToString();
        //        item.LanguageName = lan.LanguageName;
        //        item.LanguageSymbol = lan.Symbol;
        //        var res = _currencyRepository.FetchCurrency(item.CurrencyId);
        //        item.CurrencyName = res.ReturnValue.CurrencyName;
        //        item.CurrencyPrefix = res.ReturnValue.Prefix;
        //        item.CurrencySymbol = res.ReturnValue.Symbol;
        //    }
        //    RepositoryOperationResult saveResult = await _productSpecGrpRepository.Update(dto);

        //    result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
        //    : new { Status = "Error", saveResult.Message });
        //    return result;
        //}
        //[HttpGet]
        //public async Task<IActionResult> Delete(string id)
        //{
        //    RepositoryOperationResult opResult = await _productSpecGrpRepository.Delete(id, "delete");
        //    return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
        //    : new { Status = "Error", opResult.Message });
        //}
    }
}
