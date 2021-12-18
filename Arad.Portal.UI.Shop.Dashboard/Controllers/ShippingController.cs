using Arad.Portal.DataLayer.Contracts.General.BasicData;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Services;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Setting;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Shop.Setting.Mongo;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class ShippingController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBasicDataRepository _basicDataRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly ShippingSettingRepository _shippingSettingRepository;
        private readonly IDomainRepository _domainRepository;
        public ShippingController(IHttpContextAccessor accessor,
            ShippingSettingRepository shippingSettingRepository,
            UserManager<ApplicationUser> userManager,
            IDomainRepository domainRepository,
            IProviderRepository providerRepository,
            IBasicDataRepository basicDataRepository)
        {
            _shippingSettingRepository = shippingSettingRepository;
            _userManager = userManager;
            _basicDataRepository = basicDataRepository;
            _domainRepository = domainRepository;
            _providerRepository = providerRepository;
            _accessor = accessor;
        }
        public async Task<IActionResult> List()
        {
            PagedItems<ShippingSettingDTO> result;
            try
            {
                result = await _shippingSettingRepository.List(this.Request.QueryString.ToString());
                foreach (var item in result.Items)
                {
                    item.DomainName = _domainRepository.FetchDomain(item.AssociatedDomainId).ReturnValue.DomainName;
                }
            }
            catch (Exception)
            {
                result = new PagedItems<ShippingSettingDTO>();
            }
            return View(result);
        }

        public async Task<IActionResult> AddEdit(string id = "")
        {
            var model = new ShippingSettingDTO();
            //var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            //var userDB = await _userManager.FindByIdAsync(currentUserId);

            if (!string.IsNullOrWhiteSpace(id))
            {
                model = _shippingSettingRepository.FetchById(id);
            }
            foreach (var item in model.AllowedShippingTypes)
            {
                //TODO if shippingType get data from an api we should request to
                //that api based on our inputs and the get the floatingExpense
                //item.FloatingExpense = 
            }
            ViewBag.Providers = _providerRepository.GetProvidersPerType(DataLayer.Entities.General.Service.ProviderType.Shipping);
            ViewBag.ShippngTypes = _basicDataRepository.GetListPerDomain("ShippingType");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ShippingSettingDTO dto)
        {
            JsonResult result;
            if (!ModelState.IsValid)
            {
                var errors = new List<AjaxValidationErrorModel>();

                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors
                        .Select(error => new AjaxValidationErrorModel
                        { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                if(!string.IsNullOrWhiteSpace(dto.ShippingCoupon.PersianStartDate))
                {
                    dto.ShippingCoupon.StartDate = DateHelper.ToEnglishDate(dto.ShippingCoupon.PersianStartDate);
                }
                if(!string.IsNullOrWhiteSpace(dto.ShippingCoupon.PersianEndDate))
                {
                    dto.ShippingCoupon.EndDate = DateHelper.ToEnglishDate(dto.ShippingCoupon.PersianEndDate);
                }
                dto.ShippingSettingId = Guid.NewGuid().ToString();
                var domainName = _basicDataRepository.GetDomainName();
                var currentDomain = _domainRepository.FetchByName(domainName).ReturnValue;
                dto.AssociatedDomainId = currentDomain.DomainId;

                dto.ShippingCoupon.ShippingCouponId = Guid.NewGuid().ToString();
               
                Result saveResult = await _shippingSettingRepository.AddShippingSetting(dto);
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
            }
            return result;
        }
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] ShippingSettingDTO dto)
        {
            JsonResult result;
            ShippingSettingDTO shippingSetting;
            if (!ModelState.IsValid)
            {
                var errors = new List<AjaxValidationErrorModel>();

                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    errors.AddRange(modelStateVal.Errors.Select(error =>
                    new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }

                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dto.ShippingCoupon.PersianStartDate))
                {
                    dto.ShippingCoupon.StartDate = DateHelper.ToEnglishDate(dto.ShippingCoupon.PersianStartDate);
                }
                if (!string.IsNullOrWhiteSpace(dto.ShippingCoupon.PersianEndDate))
                {
                    dto.ShippingCoupon.EndDate = DateHelper.ToEnglishDate(dto.ShippingCoupon.PersianEndDate);
                }
                shippingSetting = _shippingSettingRepository.FetchById(dto.ShippingSettingId);
                if (shippingSetting == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
            }
            Result saveResult = await _shippingSettingRepository.EditShippingSetting(dto);

            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            Result opResult = await _domainRepository.DeleteDomain(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }


    }
}
