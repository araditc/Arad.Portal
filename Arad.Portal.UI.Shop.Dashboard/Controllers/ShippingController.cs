using Arad.Portal.DataLayer.Contracts.General.BasicData;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Services;
using Arad.Portal.DataLayer.Contracts.Shop.Setting;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Setting;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Policy = "Role")]
    public class ShippingController : Controller
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBasicDataRepository _basicDataRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly IShippingSettingRepository _shippingSettingRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly ICurrencyRepository _currencyRepository;
        public ShippingController(IHttpContextAccessor accessor,
            IShippingSettingRepository shippingSettingRepository,
            UserManager<ApplicationUser> userManager,
            IDomainRepository domainRepository,
            IProviderRepository providerRepository,
            ICurrencyRepository currencyRepository,
            IBasicDataRepository basicDataRepository)
        {
            _shippingSettingRepository = shippingSettingRepository;
            _userManager = userManager;
            _basicDataRepository = basicDataRepository;
            _domainRepository = domainRepository;
            _providerRepository = providerRepository;
            _currencyRepository = currencyRepository;
            _accessor = accessor;
        }
        public async Task<IActionResult> List()
        {
            PagedItems<ShippingSettingDTO> result;
            try
            {
                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userDb = await _userManager.FindByIdAsync(currentUserId);

                result = await _shippingSettingRepository.List(this.Request.QueryString.ToString(), userDb);
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
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDb = await _userManager.FindByIdAsync(currentUserId);

            var domainEntity = _domainRepository.FetchDomain(userDb.Domains.FirstOrDefault(_ => _.IsOwner).DomainId).ReturnValue;

            model.CurrencyId = domainEntity.DefaultCurrencyId;
            var defCurrency = _currencyRepository.FetchCurrency(domainEntity.DefaultCurrencyId).ReturnValue;
            model.CurrencySymbol = defCurrency.Symbol;
          
            ViewBag.IsSysAcc = userDb.IsSystemAccount;

            if (userDb.IsSystemAccount)
            {
                ViewBag.Domains = _domainRepository.GetAllActiveDomains();
            }
            model.AssociatedDomainId = domainEntity.DomainId;

            if (!string.IsNullOrWhiteSpace(id))
            {
                model = _shippingSettingRepository.FetchById(id);
                foreach (var item in model.AllowedShippingTypes)
                {
                    //TODO if HasFixedExpense = false get data from provider with providerId
                    //get expense from that provider and generate floating expense
                    //item.FloatingExpense = 
                }
            }
            ViewBag.Providers = _providerRepository.GetProvidersPerType(DataLayer.Entities.General.Service.ProviderType.Shipping);
            ViewBag.ShippngTypes = _basicDataRepository.GetList("ShippingType", true);
            ViewBag.CurrencyList = _currencyRepository.GetAllActiveCurrency();
           
            return View(model);
        }


        [HttpGet]
        public IActionResult GetSymbolOfCurrency(string currencyId)
        {
            var currencyEntity = _currencyRepository.FetchCurrency(currencyId);

            if(currencyEntity.ReturnValue != null)
            {
                return Json( new { symbol = currencyEntity.ReturnValue.Symbol });
            }else
            {
                return Json(new { symbol = "" });
            }

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
                if (!string.IsNullOrWhiteSpace(dto.ShippingCoupon.PersianStartDate))
                {
                    dto.ShippingCoupon.StartDate = DateHelper.ToEnglishDate(dto.ShippingCoupon.PersianStartDate.Split(" ")[0]);
                }
                if (!string.IsNullOrWhiteSpace(dto.ShippingCoupon.PersianEndDate))
                {
                    dto.ShippingCoupon.EndDate = DateHelper.ToEnglishDate(dto.ShippingCoupon.PersianEndDate.Split(" ")[0]);
                }
                dto.ShippingSettingId = Guid.NewGuid().ToString();
                var domainName = _domainRepository.GetDomainName();
                var currentDomain = _domainRepository.FetchByName(domainName, false).ReturnValue;
                //dto.AssociatedDomainId = currentDomain.DomainId;
                if (string.IsNullOrWhiteSpace(dto.CurrencyId))
                {
                    dto.CurrencyId = currentDomain.DefaultCurrencyId;
                }
                var currencyEntity = _currencyRepository.FetchCurrency(dto.CurrencyId).ReturnValue;
                dto.CurrencySymbol = currencyEntity.Symbol;

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
                    dto.ShippingCoupon.StartDate = DateHelper.ToEnglishDate(dto.ShippingCoupon.PersianStartDate.Split(" ")[0]);
                }
                if (!string.IsNullOrWhiteSpace(dto.ShippingCoupon.PersianEndDate))
                {
                    dto.ShippingCoupon.EndDate = DateHelper.ToEnglishDate(dto.ShippingCoupon.PersianEndDate.Split(" ")[0]);
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
            Result opResult = await _shippingSettingRepository.Delete(id);
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            Result opResult = await _domainRepository.Restore(id);
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }


    }
}
