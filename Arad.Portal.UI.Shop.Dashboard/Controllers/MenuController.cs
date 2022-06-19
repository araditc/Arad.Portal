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
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Menu;
using Microsoft.AspNetCore.Authorization;

namespace Arad.Portal.UI.Shop.Dashboard.Controllers
{
    [Authorize(Policy = "Role")]
    public class MenuController : Controller
    {
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageRepository _lanRepository;
        private readonly IProductGroupRepository _productGroupRepository;
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDomainRepository _domainRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string domainId;
        private readonly string domainName;
        public MenuController(IMenuRepository menuRepository,IContentRepository contentRepository,
            ILanguageRepository lanRepository,IHttpContextAccessor httpContextAccessor,
            IProductGroupRepository productGroupRepository, IContentCategoryRepository contentCategoryRepository,
            IProductRepository productRepository, UserManager<ApplicationUser> userManager,
            IDomainRepository domainRepository)
        {
            _menuRepository = menuRepository;
            _lanRepository = lanRepository;
            _productGroupRepository = productGroupRepository;
            _contentCategoryRepository = contentCategoryRepository;
            _httpContextAccessor = httpContextAccessor;
            _productRepository = productRepository;
            _domainRepository = domainRepository;
            _userManager = userManager;
            _contentRepository = contentRepository;
            domainName = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
            domainId = _domainRepository.FetchByName(domainName, false).ReturnValue.DomainId;
            
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            PagedItems<MenuDTO> result = new PagedItems<MenuDTO>();
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            //var userEntity = await _userManager.FindByIdAsync(currentUserId);
            //ViewBag.IsSys = userEntity.IsSystemAccount;
            
          
            var domainName = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
            var res = _domainRepository.FetchByName(domainName, false);
            var domainId = res.Succeeded ? res.ReturnValue.DomainId : Guid.Empty.ToString();
           
            try
            {
                var qs = Request.QueryString.ToString();
                if(!string.IsNullOrWhiteSpace(qs) && !qs.Contains("domainId"))
                {
                    qs += $"&domainId={domainId}";
                }else if(!qs.Contains("domainId"))
                {
                    qs = $"?domainId={domainId}";
                }
                var deflang = _lanRepository.GetDefaultLanguage(currentUserId).LanguageId;
                ViewBag.DefLangId = deflang;
                //if user is systemaccount show all active menu for all domains otherwise it only shows the menu of current domain
                ViewBag.MenuList = await _menuRepository.AllActiveMenues(domainId, deflang);
                ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
                result = await _menuRepository.AdminList(qs);
            }
            catch (Exception ex)
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
            var userEntity = await _userManager.FindByIdAsync(currentUserId);
            ViewBag.IsSys = userEntity.IsSystemAccount;

            if (userEntity.IsSystemAccount)
            {
                var domainList = _domainRepository.GetAllActiveDomains();
                ViewBag.DomainList = domainList;
            }
            var lan = _lanRepository.GetDefaultLanguage(currentUserId);
            ViewBag.DefLangId = _lanRepository.GetDefaultLanguage(currentUserId).LanguageId;
            ViewBag.ProductGroupList = await _productGroupRepository.GetAlActiveProductGroup(lan.LanguageId, currentUserId);
            ViewBag.ContentCategoryList = await _contentCategoryRepository.AllActiveContentCategory(lan.LanguageId, currentUserId);
            ViewBag.Menues = await  _menuRepository.AllActiveMenues(domainId, lan.LanguageId);
            ViewBag.MenuTypes = _menuRepository.GetAllMenuType();
            ViewBag.LangList = _lanRepository.GetAllActiveLanguage();
            
            return View(model);
        }

        [HttpGet]
        public IActionResult GetRelatedMenues(string domainId)
        {
            JsonResult result;
            List<SelectListModel> lst;
            var domainObj = _domainRepository.FetchDomain(domainId).ReturnValue;
            lst = _menuRepository.MenuesOfSelectedDomain(domainId, domainObj.DefaultLanguageId);
            if(lst.Count() > 0)
            {
                result = new JsonResult(new { Status = "success", Data = lst });
            }
            else
            {
                result = new JsonResult(new { Status = "error", Message = "" });
            }
            return result;
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
            var dbUser = await _userManager.FindByIdAsync(currentUserId);
            lst = _productRepository.GetAllProductList(dbUser, groupId, domainId);
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

                var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userEntity = await _userManager.FindByIdAsync(currentUserId);
               
                dto.AssociatedDomainId = userEntity.IsSystemAccount ? dto.AssociatedDomainId  : domainId;
                dto.MenuType = (MenuType)Convert.ToInt32(dto.MenuTypeId);
                dto.MenuCode = await GetMenuCodeFromMenu(dto.MenuType, dto.SubId, dto.SubGroupId);

                foreach (var item in dto.MenuTitles)
                {
                    var lan = _lanRepository.FetchLanguage(item.LanguageId);
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                    item.UrlFriend = await GetUrlFriend(dto.MenuType, dto.SubId, dto.SubGroupId, item.LanguageId);
                }

                switch (dto.MenuType)
                {
                    case MenuType.ProductGroup:
                        dto.Url = $"/group/{dto.MenuCode}";
                        break;
                    case MenuType.Product:
                        dto.Url = $"/product/{dto.MenuCode}";
                        break;
                    case MenuType.CategoryContent:
                        dto.Url = $"/category/{dto.MenuCode}";
                        break;
                    case MenuType.Content:
                        dto.Url = $"/blog/{dto.MenuCode}";
                        break;
                }

                Result saveResult = await _menuRepository.AddMenu(dto);
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
            dto.MenuType = (MenuType)Convert.ToInt32(dto.MenuTypeId);
            dto.MenuCode = await GetMenuCodeFromMenu(dto.MenuType, dto.SubId, dto.SubGroupId);
            foreach (var item in dto.MenuTitles)
            {
                var lan = _lanRepository.FetchLanguage(item.LanguageId);
                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                item.LanguageName = lan.LanguageName;
                item.LanguageSymbol = lan.Symbol;
                item.UrlFriend = await GetUrlFriend(dto.MenuType, dto.SubId, dto.SubGroupId, item.LanguageId);
            }
            
            
            switch (dto.MenuType)
            {
                case MenuType.ProductGroup:
                    dto.Url = $"/group/{dto.MenuCode}";
                    break;
                case MenuType.Product:
                    dto.Url = $"/product/{dto.MenuCode}";
                    break;
                case MenuType.CategoryContent:
                    dto.Url = $"/category/{dto.MenuCode}";
                    break;
                case MenuType.Content:
                    dto.Url = $"/blog/{dto.MenuCode}";
                    break;
                //case MenuType.DirectLink:
                //    break;
                //case MenuType.Module:
                //    break;
                //default:
                //    break;
            }
            Result saveResult = await _menuRepository.EditMenu(dto);

            result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
            : new { Status = "Error", saveResult.Message });
            return result;
        }
        private async Task<string> GetUrlFriend(MenuType type, string subId, string subGroupId, string lanId)
        {
            var res = "";
            switch (type)
            {
                case MenuType.ProductGroup:
                    var grp = _productGroupRepository.ProductGroupFetch(subGroupId);
                    res = grp.MultiLingualProperties.Any(_=>_.LanguageId == lanId) ? grp.MultiLingualProperties.FirstOrDefault(_ => _.LanguageId == lanId).UrlFriend : "" ;
                    break;
                case MenuType.Product:
                    var pro = await _productRepository.ProductFetch(subId);
                    res = pro.MultiLingualProperties.Any(_=>_.LanguageId == lanId) ? pro.MultiLingualProperties.FirstOrDefault(_=>_.LanguageId == lanId).UrlFriend : "";
                    break;
                case MenuType.CategoryContent:
                    var category = await _contentCategoryRepository.ContentCategoryFetch(subGroupId);
                    res = category.CategoryNames.Any(_=>_.LanguageId == lanId) ? category.CategoryNames.FirstOrDefault(_ => _.LanguageId == lanId).UrlFriend : "";
                    break;
                case MenuType.Content:
                    var content = await _contentRepository.ContentFetch(subId);
                    res = content.LanguageId == lanId ? content.UrlFriend : "";
                    break;
                    //case MenuType.DirectLink:
                    //    break;
                    //case MenuType.Module:
                    //    break;
                    //default:
                    //    break;
            }
            return res;
        }
        private async Task<long> GetMenuCodeFromMenu(MenuType type, string subId,string subGroupId)
        {
            long res = 0;
            switch (type)
            {
                case MenuType.ProductGroup:
                    var grp = _productGroupRepository.ProductGroupFetch(subGroupId);
                    res = grp.GroupCode;
                    break;
                case MenuType.Product:
                    var pro = await _productRepository.ProductFetch(subId);
                    res = pro.ProductCode;
                    break;
                case MenuType.CategoryContent:
                    var category = await _contentCategoryRepository.ContentCategoryFetch(subGroupId);
                    res =  category.CategoryCode;
                    break;
                case MenuType.Content:
                    var content = await _contentRepository.ContentFetch(subId);
                    res = content.ContentCode;
                    break;
                //case MenuType.DirectLink:
                //    break;
                //case MenuType.Module:
                //    break;
                //default:
                //    break;
            }
            return res;
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            Result opResult = await _menuRepository.DeleteMenu(id);
            return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
            : new { Status = "Error", opResult.Message });
        }
    }
}
