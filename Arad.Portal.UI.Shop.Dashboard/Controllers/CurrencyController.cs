using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.ContentCategory;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Models.Currency;
using System.Collections.Generic;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class CurrencyController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrencyRepository _currencyRepository;

        public CurrencyController(
            ICurrencyRepository currencyRepository,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _currencyRepository = currencyRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<CurrencyDTO> result = new PagedItems<CurrencyDTO>();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDb = await _userManager.FindByIdAsync(currentUserId);
            try
            {
                result = await _currencyRepository.AllCurrencyList(Request.QueryString.ToString());
            }
            catch (Exception)
            {
            }
            return View(result);
        }


       
        public IActionResult AddEdit(string id = "")
        {
            var model = new CurrencyDTO();

            if (!string.IsNullOrWhiteSpace(id))
            {
                model = _currencyRepository.FetchCurrency(id).ReturnValue;
            }
            return View(model);
        }
        

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] CurrencyDTO dto)
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
                
                Result saveResult = await _currencyRepository.SaveCurrency(dto);
               
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
            }
            return result;

        }
        
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            Result opResult = await _currencyRepository.DeleteCurrency(id);
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Restore(string id)
        {
            Result opResult = await _currencyRepository.RestoreCurrency(id);
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }
    }
}
