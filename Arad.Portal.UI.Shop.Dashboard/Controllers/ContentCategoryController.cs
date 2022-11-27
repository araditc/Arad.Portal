using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Currency;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.ContentCategory;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Arad.Portal.UI.Shop.Dashboard.Helpers;
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
    public class ContentCategoryController : Controller
    {
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly CodeGenerator _codeGenerator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDomainRepository _domainRepository;
       
        public ContentCategoryController(IContentCategoryRepository contentCategoryRepository,
            CodeGenerator codeGenerator,
            IDomainRepository domainRepository,
            UserManager<ApplicationUser> userManager,
            ILanguageRepository lanRepository)
        {
            _contentCategoryRepository = contentCategoryRepository;
            _lanRepository = lanRepository;
            _codeGenerator = codeGenerator;
            _userManager = userManager;
            _domainRepository = domainRepository;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<ContentCategoryViewModel> result = new PagedItems<ContentCategoryViewModel>();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDb = await _userManager.FindByIdAsync(currentUserId);
            try
            {
                result = await _contentCategoryRepository.List(Request.QueryString.ToString(), userDb);
                ViewBag.DefLangId = _lanRepository.GetDefaultLanguage(currentUserId).LanguageId;
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            }
            catch (Exception)
            {
            }
            return View(result);
        }


        [HttpGet]
        public IActionResult CheckUrlFriendUniqueness(string id, string url)
        {
            var urlFriend = $"/category/{url}";
            var res = _contentCategoryRepository.IsUniqueUrlFriend(urlFriend, id);

            return Json(res ? new { Status = "Success", Message = "url is unique" }
            : new { Status = "Error", Message = "url isnt unique" });
        }
        public async Task<IActionResult> AddEdit(string id = "")
        {
            var model = new ContentCategoryDTO();

            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDB = await _userManager.FindByIdAsync(currentUserId);
            if (userDB.IsSystemAccount)
            {
                ViewBag.Domains = _domainRepository.GetAllActiveDomains();
            }
           
            model.AssociatedDomainId = userDB.Domains.FirstOrDefault(_ => _.IsOwner).DomainId;
           
            ViewBag.IsSysAcc = userDB.IsSystemAccount;
            var lan = _lanRepository.GetDefaultLanguage(currentUserId);
            ViewBag.LangId = lan.LanguageId;

            if (!string.IsNullOrWhiteSpace(id))
            {
                model = await _contentCategoryRepository.ContentCategoryFetch(id, true);
            }
            else
            {
                model.CategoryCode = _codeGenerator.GetNewId();
            }
            var categoryList = await _contentCategoryRepository.AllActiveContentCategory(lan.LanguageId, currentUserId, model.AssociatedDomainId);
            categoryList.Insert(0, new SelectListModel() { Text = Language.GetString("AlertAndMessage_Choose"), Value = "-1" });
            ViewBag.CategoryList = categoryList;
            var lst =  _contentCategoryRepository.GetAllContentCategoryType();

         
            ViewBag.CategoryTypes = lst;
          

            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();          
            return View(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lid">languageId</param>
        /// <param name="did">domainId</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetContentCategoryList(string lid, string did)
        {
            JsonResult result;
            List<SelectListModel> lst = new List<SelectListModel>();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(!string.IsNullOrWhiteSpace(did))
            {
                lst = await _contentCategoryRepository.AllActiveContentCategory(lid, currentUserId, did);
            }
            
            if (lst.Count > 0)
            {
                result = new JsonResult(new { Status = "success", Data = lst.OrderBy(_=>_.Text) });
            }
            else
            {
                result = new JsonResult(new { Status = "error", Message = Language.GetString(ConstMessages.ObjectNotFound) });
            }
            return result;

        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ContentCategoryDTO dto)
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
                foreach (var item in dto.CategoryNames)
                {
                    var lan = _lanRepository.FetchLanguage(item.LanguageId);
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                }

                Result saveResult = await _contentCategoryRepository.Add(dto);
                if(saveResult.Succeeded)
                {
                    _codeGenerator.SaveToDB(dto.CategoryCode);
                }
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
                var dto = _contentCategoryRepository.ContentCategoryFetch(id, true);
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
                    var res = await _contentCategoryRepository.Restore(id);
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
        public async Task<IActionResult> Edit([FromBody] ContentCategoryDTO dto)
        {
            JsonResult result;
            ContentCategoryDTO model;
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
                model =await _contentCategoryRepository.ContentCategoryFetch(dto.ContentCategoryId, true);
                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
            }

            foreach (var item in dto.CategoryNames)
            {
                var lan = _lanRepository.FetchLanguage(item.LanguageId);
                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                item.LanguageName = lan.LanguageName;
                item.LanguageSymbol = lan.Symbol;
            }

            Result saveResult = await _contentCategoryRepository.Update(dto);

            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            Result opResult = await _contentCategoryRepository.Delete(id, "delete");
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }
    }
}
