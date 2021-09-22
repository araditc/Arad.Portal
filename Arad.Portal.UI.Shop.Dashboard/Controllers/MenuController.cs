using Arad.Portal.DataLayer.Contracts.General.ContentCategory;
using Arad.Portal.DataLayer.Contracts.General.Language;
using Arad.Portal.DataLayer.Contracts.General.Menu;
using Arad.Portal.DataLayer.Contracts.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Menu;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.UI.Shop.Dashboard.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Http;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Microsoft.AspNetCore.Identity;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Contracts.General.Content;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    public class MenuController : Controller
    {
        private readonly IMenuRepository _menuRepository;
        private readonly IPermissionView _permissionViewManager;
        private readonly ILanguageRepository _lanRepository;
        private readonly IProductGroupRepository _productGroupRepository;
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string domainId;
        public MenuController(IMenuRepository menuRepository,IContentRepository contentRepository,
            IPermissionView permissionView, ILanguageRepository lanRepository,IHttpContextAccessor httpContextAccessor,
            IProductGroupRepository productGroupRepository, IContentCategoryRepository contentCategoryRepository,
            IProductRepository productRepository, UserManager<ApplicationUser> userManager)
        {
            _menuRepository = menuRepository;
            _permissionViewManager = permissionView;
            _lanRepository = lanRepository;
            _productGroupRepository = productGroupRepository;
            _contentCategoryRepository = contentCategoryRepository;
            _httpContextAccessor = httpContextAccessor;
            _productRepository = productRepository;
            _userManager = userManager;
            _contentRepository = contentRepository;
            domainId = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<MenuDTO> result = new PagedItems<MenuDTO>();
            var dicKey = await _permissionViewManager.PermissionsViewGet(HttpContext);
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.Permissions = dicKey;
            try
            {
                result = await _menuRepository.AdminList(Request.QueryString.ToString());
                ViewBag.DefLangId = _lanRepository.GetDefaultLanguage(currentUserId).LanguageId;
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            }
            catch (Exception)
            {
            }
            return View(result);
        }
        public async Task<IActionResult> AddEdit(string id = "")
        {
            var model = new MenuDTO();
            if (!string.IsNullOrWhiteSpace(id))
            {
                var res = _menuRepository.FetchMenu(id);
                model = res.ReturnValue;
            }
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var lan = _lanRepository.GetDefaultLanguage(currentUserId);
            ViewBag.LangId = lan.LanguageId;
            ViewBag.ProductGroupList = await _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId, currentUserId);
            ViewBag.ContentCategoryList = await _contentCategoryRepository.AllActiveContentCategory(lan.LanguageId, currentUserId);
            ViewBag.Menues = _menuRepository.AllActiveMenues(domainId, lan.LanguageId);
            ViewBag.MenuTypes = _menuRepository.GetAllMenuType();
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            
            return View(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">id stands for langId</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProductList(string groupId)
        {
            JsonResult result;
            List<SelectListModel> lst;
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDb = await _userManager.FindByIdAsync(currentUserId);
            if(userDb.IsSystemAccount)
            {
                currentUserId = Guid.Empty.ToString();
            }
           
            lst = _productRepository.GetAllProductList(currentUserId, groupId, domainId);
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
        public async Task<IActionResult> GetContentList(string categoryId)
        {
            JsonResult result;
            List<SelectListModel> lst;
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userDb = await _userManager.FindByIdAsync(currentUserId);
            if (userDb.IsSystemAccount)
            {
                currentUserId = Guid.Empty.ToString();
            }
            lst = _contentRepository.GetContentsList(domainId, currentUserId, categoryId);
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
        public async Task<IActionResult> Add([FromBody] MenuDTO dto)
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
                foreach (var item in dto.MenuTitles)
                {
                    var lan = _lanRepository.FetchLanguage(item.LanguageId);
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                }

                RepositoryOperationResult saveResult = await _menuRepository.AddMenu(dto);
                result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                : new { Status = "Error", saveResult.Message });
            }
            return result;

        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] MenuDTO dto)
        {
            JsonResult result;
            MenuDTO model;
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
                model = _menuRepository.FetchMenu(dto.MenuId).ReturnValue;
                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }
            }

            foreach (var item in dto.MenuTitles)
            {
                var lan = _lanRepository.FetchLanguage(item.LanguageId);
                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                item.LanguageName = lan.LanguageName;
                item.LanguageSymbol = lan.Symbol;
            }

            RepositoryOperationResult saveResult = await _menuRepository.EditMenu(dto);

            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            RepositoryOperationResult opResult = await _menuRepository.DeleteMenu(id);
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }
    }
}
