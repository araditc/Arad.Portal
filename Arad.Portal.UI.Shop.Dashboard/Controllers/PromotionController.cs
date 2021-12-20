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
using System.Collections.Generic;
using System.Linq;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Shop.Product.Mongo;
using Microsoft.AspNetCore.Authorization;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class PromotionController : Controller
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly ILanguageRepository _lanRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IProductRepository _productRepositoy;
        private readonly UserManager<ApplicationUser> _userManager;
        public PromotionController(IPromotionRepository promotionRepository,
            IPermissionView permissionView, ILanguageRepository lanRepository, 
            IProductRepository productRepositoy,
            ICurrencyRepository currencyRepository, UserManager<ApplicationUser> userManager)
        {
            _promotionRepository = promotionRepository;
            _permissionViewManager = permissionView;
            _lanRepository = lanRepository;
            _currencyRepository = currencyRepository;
            _userManager = userManager;
            _productRepositoy = productRepositoy;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<PromotionDTO> list = new PagedItems<PromotionDTO>();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            ViewBag.IsSysAcc = currentUser.IsSystemAccount;
            var defaulltLang = _lanRepository.GetDefaultLanguage(currentUserId);
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            ViewBag.Permissions = dicKey;
            var lst = _productRepositoy.GetProductsOfThisVendor(defaulltLang.LanguageId, currentUserId);
            lst.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
            ViewBag.CurrentSellerProductList = lst;
           
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
                ViewBag.PromotionTypes = _promotionRepository.GetAllPromotionType();
                ViewBag.DiscountTypes = _promotionRepository.GetAllDiscountType();
                ViewBag.ProductGroupList = _productRepositoy.GetAlActiveProductGroup(defaulltLang.LanguageId);
                var vendorList = await _userManager.GetUsersForClaimAsync(new Claim("AppRole", "True"));
                ViewBag.Vendors = vendorList.ToList().Select(_ => new SelectListModel()
                {
                    Text = _.Profile.FullName,
                    Value = _.Id
                }).ToList();
                

            }
            catch (Exception)
            {
            }
            return View(list);
        }

        public  async Task<IActionResult> AddEdit(string id)
        {
            var model = new PromotionDTO();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var defaulltLang = _lanRepository.GetDefaultLanguage(currentUserId);
            var userDB = await _userManager.FindByIdAsync(currentUserId);
            ViewBag.IsSysAcc = userDB.IsSystemAccount;
            if(!userDB.IsSystemAccount)
            {
                var lst = _productRepositoy.GetProductsOfThisVendor(defaulltLang.LanguageId, currentUserId);
                lst.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
                ViewBag.CurrentSellerProductList = lst;
            }
            if (!string.IsNullOrWhiteSpace(id))
            {
                model = _promotionRepository.FetchPromotion(id);
            }

            ViewBag.DefCurrencyId = _currencyRepository.GetDefaultCurrency(currentUserId);
            ViewBag.CurrencyList = _currencyRepository.GetAllActiveCurrency();
            ViewBag.DiscountTypes = _promotionRepository.GetAllDiscountType();
            ViewBag.PromotionTypes = _promotionRepository.GetAllPromotionType();
            
            ViewBag.ProductGroupList = _productRepositoy.GetAlActiveProductGroup(defaulltLang.LanguageId);
            var vendorList = await _userManager.GetUsersForClaimAsync(new Claim("AppRole", "True"));
            ViewBag.Vendors = vendorList.ToList().Select(_ => new SelectListModel()
            {
                Text = _.Profile.FullName,
                Value = _.Id
            });
            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> Add([FromBody] PromotionDTO dto)
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
                
               
                Result saveResult = await _promotionRepository.InsertPromotion(dto);
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

                var res = await _promotionRepository.Restore(id);
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


        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] PromotionDTO dto)
        {
            JsonResult result;
            PromotionDTO model;
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
                model = _promotionRepository.FetchPromotion(dto.PromotionId);
                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
            }

            
            Result saveResult = await _promotionRepository.UpdatePromotion(dto);

            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            Result opResult = await _promotionRepository.DeletePromotion(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetFilteredProduct(string productGroupId, string vendorId)
        {
            JsonResult result;
            List<SelectListModel> lst;
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDb = await _userManager.FindByIdAsync(currentUserId);
            var defaultLanguage = _lanRepository.GetDefaultLanguage(currentUserId);
            if (vendorId == "0")
            {
                if (!userDb.IsSystemAccount)
                {
                    vendorId = currentUserId;
                }
            }
            if (userDb.IsSystemAccount)
            {
                vendorId = "-1";
            }
            if (userDb.IsSystemAccount) currentUserId = Guid.Empty.ToString();
            lst = _productRepositoy.GetAllActiveProductList(defaultLanguage.LanguageId, currentUserId, productGroupId, vendorId);
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


        [HttpGet]
        public async Task<IActionResult> GetFilteredProductGroup(string vendorId)
        {
            JsonResult result;
            List<SelectListModel> lst;
            
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDb = await _userManager.FindByIdAsync(currentUserId);
            if (vendorId == "0")
            {
                if(!userDb.IsSystemAccount)
                {
                    vendorId = currentUserId;
                }
            }
            if(userDb.IsSystemAccount)
            {
                vendorId = "-1";
            }
            var defaultLanguage = _lanRepository.GetDefaultLanguage(currentUserId);
            var list = _productRepositoy.GetGroupsOfThisVendor(vendorId, defaultLanguage.LanguageId);
            if (list.Count() > 0)
            {
                result = new JsonResult(new { Status = "success", Data = list });
            }
            else
            {
                result = new JsonResult(new { Status = "notFound", Message = "" });
            }
            return result;
        }
    }
}
