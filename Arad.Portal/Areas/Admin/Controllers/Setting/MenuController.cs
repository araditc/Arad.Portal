using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.Menu;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.ProductGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Shared.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Content;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.ContentCategory;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Menu;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductGroup;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Menu;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using MongoDB.Driver;

using Serilog;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class MenuController : Controller
{
    private readonly IContentCategoryRepository _contentCategoryRepository;
    private readonly IModificationRepository _modificationRepository;
    private readonly IContentRepository _contentRepository;
    private readonly ControllerHelper _controllerHelper;
    private readonly string _domainId;
    private readonly IDomainRepository _domainRepository;
    private readonly ILanguageRepository _lanRepository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly IMenuRepository _menuRepository;
    private readonly IProductGroupRepository _productGroupRepository;
    private readonly IProductRepository _productRepository;

    public MenuController(IMenuRepository menuRepository,
                          IContentRepository contentRepository,
                          ILanguageRepository lanRepository,
                          IHttpContextAccessor httpContextAccessor,
                          IProductGroupRepository productGroupRepository,
                          IContentCategoryRepository contentCategoryRepository,
                          IModificationRepository modificationRepository,
                          IProductRepository productRepository,
                          ILogger logger,
                          IDomainRepository domainRepository,
                          IMapper mapper,
                          ControllerHelper controllerHelper)
    {
        _menuRepository = menuRepository;
        _lanRepository = lanRepository;
        _productGroupRepository = productGroupRepository;
        _contentCategoryRepository = contentCategoryRepository;
        _modificationRepository = modificationRepository;
        _productRepository = productRepository;
        _domainRepository = domainRepository;
        _mapper = mapper;
        _controllerHelper = controllerHelper;
        _logger = logger;
        _contentRepository = contentRepository;

        if (httpContextAccessor.HttpContext == null)
        {
            return;
        }

        string domainName = $"{httpContextAccessor.HttpContext.Request.Host}";
        _domainId = _controllerHelper.FetchDomainByName(domainName, false).ReturnValue.Id;
    }

    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        PagedItems<MenuDto> menuDtoRes = new();

        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            Language defLang = _controllerHelper.GetDefaultLanguage();
            ViewBag.DefLangId = defLang.Id;

            //if user is system account show all active menu for all domains otherwise it only shows the menu of current domain
            List<SelectListModel> menuList;

            if (userDb.IsSystemAccount)
            {
                menuList = (await _menuRepository.GetListAsync(m => m.IsActive && !m.IsDeleted, cancellationToken))
                           .Select(m => new SelectListModel { Value = m.Id.ToString(), Text = m.MenuTitles.Any(a => a.LanguageId == defLang.Id) ? m.MenuTitles.FirstOrDefault(a => a.LanguageId == defLang.Id)?.Name : "" })
                           .ToList();
            }
            else
            {
                menuList = (await _menuRepository.GetListAsync(m => m.IsActive && m.AssociatedDomainId == _domainId, cancellationToken))
                           .Select(m => new SelectListModel { Value = m.Id.ToString(), Text = m.MenuTitles.Any(a => a.LanguageId == defLang.Id) ? m.MenuTitles.FirstOrDefault(a => a.LanguageId == defLang.Id)?.Name : "" })
                           .ToList();
            }

            menuList.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "-1" });
            ViewBag.MenuList = menuList;
            ViewBag.LangList = _controllerHelper.GetAllActiveLanguage();
            ViewBag.IsSystemAccount = userDb.IsSystemAccount;

            if (userDb.IsSystemAccount)
            {
                ViewBag.Domains = _controllerHelper.GetAllActiveDomains();
            }
            string queryString = Request.QueryString.ToString();
            string languageId;
            NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

            if (string.IsNullOrWhiteSpace(filter["page"]))
            {
                filter.Set("page", "1");
            }

            if (string.IsNullOrWhiteSpace(filter["PageSize"]))
            {
                filter.Set("PageSize", "20");
            }

            int page = Convert.ToInt32(filter["page"]);
            int pageSize = Convert.ToInt32(filter["PageSize"]);

            string parentId = "";
            string keyToFilter = "";

            if (string.IsNullOrWhiteSpace(filter["LanguageId"]))
            {
                Language lan = await _lanRepository.FirstOrDefaultAsync(l => l.IsDefault, cancellationToken);
                languageId = lan.Id;
                filter.Set("LanguageId", lan.Id);
            }
            else
            {
                languageId = filter["LanguageId"]!;
            }

            if (!string.IsNullOrWhiteSpace(filter["Id"]))
            {
                parentId = filter["Id"]!;
            }

            if (!string.IsNullOrWhiteSpace(filter["filter"]))
            {
                keyToFilter = filter["filter"]!;
            }

            string domainEntityId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
            FilterDefinitionBuilder<Menu> menuBuilder = new();
            FilterDefinitionBuilder<MultiLingualProperty> menuTitlesBuilder = new();
            FilterDefinition<Menu> menuFilterDef = userDb.IsSystemAccount ? menuBuilder.Empty : menuBuilder.Eq(nameof(Menu.AssociatedDomainId), domainEntityId);

            menuFilterDef = menuBuilder.And(menuFilterDef, menuBuilder.Eq(nameof(Menu.IsActive), true));

            if (parentId != "")
            {
                menuFilterDef = menuBuilder.And(menuFilterDef, menuBuilder.Eq(nameof(Menu.ParentId), parentId));
            }

            if (keyToFilter != "")
            {
                FilterDefinition<MultiLingualProperty> menuTitleFilterDef = menuTitlesBuilder.Regex(p => p.Name, new($".*{keyToFilter}.*"));
                menuFilterDef = menuBuilder.And(menuFilterDef, menuBuilder.ElemMatch("MenuTitles", menuTitleFilterDef));
            }

            long totalCount = await _menuRepository.GetCountAsync(c => c.IsActive == true, cancellationToken);
            List<MenuDto> lst = (await _menuRepository.GetAllAsync(cancellationToken))
                                .Select(m =>
                                            new MenuDto
                                            {
                                                Id = m.Id,
                                                Icon = m.Icon,
                                                MenuTitle = m.MenuTitles.Any(p => p.LanguageId == languageId) ? m.MenuTitles.FirstOrDefault(p => p.LanguageId == languageId)?.Name : m.MenuTitles.First().Name,
                                                LanguageId = languageId,
                                                MenuTitles = m.MenuTitles,
                                                MenuType = m.MenuType,
                                                Order = m.Order,
                                                ParentName = m.ParentName,
                                                ParentId = m.ParentId,
                                                Url = m.Url,
                                                SubId = m.SubId,
                                                SubName = m.SubName,
                                                SubGroupId = m.SubGroupId,
                                                SubGroupName = m.SubGroupName,
                                                CreatorUserName = m.CreatorUserName,
                                                CreatorUserId = m.CreatorUserId,
                                                IsDeleted = m.IsDeleted
                                            })
                                .ToList(); /*.Sort(Builders<Menu>.Sort.Ascending(a => a.Order)).Skip((page - 1) * pageSize).Limit(pageSize).ToList();*/
            menuDtoRes.CurrentPage = page;
            menuDtoRes.Items = lst;
            menuDtoRes.ItemsCount = totalCount;
            menuDtoRes.PageSize = pageSize;
            menuDtoRes.QueryString = queryString;
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(MenuController)}/{nameof(List)}");
        }

        return View(menuDtoRes);
    }

    public async ValueTask<IActionResult> AddEdit(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            MenuDto model = new();

            ViewBag.IsSysAcc = userDb.IsSystemAccount;

            if (userDb.IsSystemAccount)
            {
                List<SelectListModel> domainList = _controllerHelper.GetAllActiveDomains();
                ViewBag.DomainList = domainList;
            }

            model.AssociatedDomainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;

            if (!string.IsNullOrWhiteSpace(id))
            {
                Result<MenuDto> result = new();
                Menu entity = await _menuRepository.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

                try
                {
                    if (entity != null)
                    {
                        MenuDto dto = _mapper.Map<MenuDto>(entity);
                        result.Succeeded = true;
                        result.Message = ConstMessages.SuccessfullyDone;
                        result.ReturnValue = dto;
                    }
                    else
                    {
                        result.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
                    }
                }
                catch (Exception)
                {
                    result.Message = ConstMessages.ExceptionOccured;
                }

                model = result.ReturnValue;
            }

            //Language lan = _controllerHelper.GetDefaultLanguage();
            string? lanId = _controllerHelper.GetCurrentUserDomain().DefaultLanguageId;
            Language lan = _controllerHelper.FetchLanguage(lanId);
            ViewBag.DefLangId = lan.Id;

            ViewBag.ProductGroupList = _controllerHelper.GetAllActiveProductGroup(lan.Id);

            ViewBag.ContentCategoryList = _controllerHelper.AllActiveContentCategory(lan.Id);

            List<SelectListModel> menuList;

            if (userDb.IsSystemAccount)
            {
                menuList = (await _menuRepository.GetListAsync(m => m.IsActive && !m.IsDeleted, cancellationToken))
                           .Select(m => new SelectListModel { Value = m.Id.ToString(), Text = m.MenuTitles.Any(a => a.LanguageId == lan.Id) ? m.MenuTitles.FirstOrDefault(a => a.LanguageId == lan.Id)?.Name : "" })
                           .Where(item => !string.IsNullOrEmpty(item.Text))
                           .ToList();
            }
            else
            {
                menuList = (await _menuRepository.GetListAsync(m => m.IsActive && m.AssociatedDomainId == _controllerHelper.GetCurrentUserDomain().Id, cancellationToken))
                           .Select(m => new SelectListModel { Value = m.Id.ToString(), Text = m.MenuTitles.Any(a => a.LanguageId == lan.Id) ? m.MenuTitles.FirstOrDefault(a => a.LanguageId == lan.Id)?.Name : "" })
                           .ToList();
            }

            menuList.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "-1" });

            ViewBag.Menues = menuList;

            ViewBag.LangList = _controllerHelper.GetAllActiveLanguage();

            List<SelectListModel> menuTypeList = [];
            menuTypeList.AddRange(from int i in Enum.GetValues(typeof(MenuType)) let name = Enum.GetName(typeof(MenuType), i) select new SelectListModel { Text = name, Value = i.ToString() });

            menuTypeList.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });
            ViewBag.MenuTypes = menuTypeList;

            return View(model);
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(MenuController)}/{nameof(AddEdit)}");
        }

        return View();
    }

    [HttpGet]
    public async ValueTask<IActionResult> GetRelatedMenues(string domainId, CancellationToken cancellationToken)
    {
        JsonResult result;
        List<SelectListModel> menuSelectedList = [];
        Domain domainObj = _controllerHelper.FetchDomain(domainId).ReturnValue;

        if (!string.IsNullOrEmpty(domainId))
        {
            menuSelectedList = (await _menuRepository.GetListAsync(m => m.IsActive && m.AssociatedDomainId == domainId, cancellationToken))
                               .Select(m => new SelectListModel
                                            {
                                                Value = m.Id.ToString(),
                                                Text = m.MenuTitles.Any(a => a.LanguageId == domainObj.DefaultLanguageId) ? m.MenuTitles.FirstOrDefault(a => a.LanguageId == domainObj.DefaultLanguageId)?.Name : ""
                                            })
                               .ToList();

            menuSelectedList.Insert(0, new() { Text = UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "-1" });
        }

        if (menuSelectedList.Count > 0)
        {
            result = new(new { Status = "success", Data = menuSelectedList });
        }
        else
        {
            result = new(new { Status = "error", Message = UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
        }

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> GetProductList(string groupId, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);
        List<SelectListModel> productList = [];
        Domain domainEntity = await _domainRepository.AnyAsync(d => d.Id == _domainId, cancellationToken)
                                  ? await _domainRepository.FirstOrDefaultAsync(d => d.Id == _domainId, cancellationToken)
                                  : new();

        if (userDb.IsSystemAccount) //systemAccount
        {
            productList = (await _productRepository.GetListAsync(p => p.GroupIds.Contains(groupId) && p.IsActive, cancellationToken))
                                            .Select(p => new SelectListModel
                                                         {
                                                             Text = p.MultiLingualProperties.Count(a => a.LanguageId == userDb.Profile.DefaultLanguageId) != 0
                                                                        ? p.MultiLingualProperties.FirstOrDefault(a => a.LanguageId == userDb.Profile.DefaultLanguageId)?.Name
                                                                        : p.MultiLingualProperties.FirstOrDefault()?.Name,
                                                             Value = p.Id.ToString()
                                                         })
                                            .ToList();
        }
        else
        {
            if (domainEntity != null)
            {
                productList = (await _productRepository.GetListAsync(p => p.GroupIds.Contains(groupId) && p.IsActive && p.CreatorUserId == userDb.Id, cancellationToken))
                                                .Select(p => new SelectListModel
                                                             {
                                                                 Text = p.MultiLingualProperties.Count(a => a.LanguageId == domainEntity.DefaultLanguageId) != 0
                                                                            ? p.MultiLingualProperties.FirstOrDefault(a => a.LanguageId == domainEntity.DefaultLanguageId)?.Name
                                                                            : p.MultiLingualProperties.FirstOrDefault()?.Name,
                                                                 Value = p.Id.ToString()
                                                             })
                                                .ToList();
            }
        }

        if (productList.Any())
        {
            result = new(new { Status = "success", Data = productList });
        }
        else
        {
            result = new(new { Status = "error", Message = UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
        }

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> GetContentList(string categoryId, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);
        string domainId = userDb.Domains.FirstOrDefault(c => c.IsOwner)?.DomainId;
        if (userDb.IsSystemAccount)
        {
            userDb.Id = Guid.NewGuid().ToString();
        }

        List<SelectListModel> lst = _controllerHelper.GetContentsList(domainId, categoryId);

        if (lst.Count > 0)
        {
            result = new(new { Status = "success", Data = lst });
        }
        else
        {
            result = new(new { Status = "error", Message = UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
        }

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Add([FromBody] MenuDto dto, CancellationToken cancellationToken)
    {
        JsonResult result;
        Result<Menu> saveResult = new();
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            if (!ModelState.IsValid)
            {
                List<AjaxValidationErrorModel> errors = [];

                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey];

                    if (modelStateVal != null)
                    {
                        errors.AddRange(modelStateVal.Errors
                                                     .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                    }
                }

                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                dto.MenuType = int.Parse(dto.MenuTypeId) switch
                               {
                                   0 => MenuType.ProductGroup,
                                   1 => MenuType.Product,
                                   2 => MenuType.CategoryContent,
                                   3 => MenuType.Content,
                                   4 => MenuType.DirectLink,
                                   5 => MenuType.Module,
                                   _ => dto.MenuType
                               };
                Menu model = _mapper.Map<Menu>(dto);

                model.AssociatedDomainId = userDb.IsSystemAccount ? model.AssociatedDomainId : _controllerHelper.GetCurrentUserDomain().Id;
                model.MenuType = (MenuType)Convert.ToInt32(model.MenuType);
                model.MenuCode = await GetMenuCodeFromMenu(model.MenuType, model.SubId, model.SubGroupId);

                foreach (MultiLingualProperty item in model.MenuTitles)
                {
                    Language lan = _controllerHelper.FetchLanguage(item.LanguageId);
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                    item.UrlFriend = await GetUrlFriend(dto.MenuType, dto.SubId, dto.SubGroupId, item.LanguageId, cancellationToken);
                }

                model.Url = model.MenuType switch
                            {
                                MenuType.ProductGroup => $"/group/{model.MenuCode}",
                                MenuType.Product => $"/product/{model.MenuCode}",
                                MenuType.CategoryContent => $"/category/{model.MenuCode}",
                                MenuType.Content => $"/blog/{model.MenuCode}",
                                _ => model.Url
                            };

                model.Id = Guid.NewGuid().ToString();
                model.CreationDate = DateTime.Now;
                model.CreatorUserId = userDb.Id;
                model.CreatorUserName = userDb.UserName;
                model.IsActive = true;

                saveResult = await _menuRepository.InsertAsync(model, cancellationToken);

                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    ActionTypes = ActionTypes.Insert,
                                                    CollectionType = CollectionType.Menu,
                                                    Ip = _controllerHelper.GetUserIpAddress(),
                                                    ModifierId = userDb.Id,
                                                    ModifierUserName = userDb.UserName,
                                                    ModifyDateTime = DateTime.Now,
                                                    RecordId = model.Id
                                                };
                    Result<Modification> modifyInsert = await _modificationRepository.InsertAsync(modification, cancellationToken);

                    _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding menu with {dto.Id} id and {dto.MenuTitle} done successfully");
                    saveResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    saveResult.Message = ConstMessages.ErrorInSaving;
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(MenuController)}/{nameof(Add)}");
            saveResult.Message = ConstMessages.ExceptionOccured;
        }

        result = Json(saveResult.Succeeded
                          ? new { Status = "Success", saveResult.Message }
                          : new { Status = "Error", saveResult.Message });

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Edit([FromBody] MenuDto dto, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);
        Result<Menu> saveResult = new();
        JsonResult result;

        try
        {
            
            if (!ModelState.IsValid)
            {
                List<AjaxValidationErrorModel> errors = [];

                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey];

                    if (modelStateVal != null)
                    {
                        errors.AddRange(modelStateVal.Errors.Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                    }
                }

                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                Menu menu = await _menuRepository.FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

                if (menu == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }

                dto.MenuType = int.Parse(dto.MenuTypeId) switch
                               {
                                   0 => MenuType.ProductGroup,
                                   1 => MenuType.Product,
                                   2 => MenuType.CategoryContent,
                                   3 => MenuType.Content,
                                   4 => MenuType.DirectLink,
                                   5 => MenuType.Module,
                                   _ => dto.MenuType
                               };

                Menu modelMenu = _mapper.Map(dto, menu);

                modelMenu.MenuType = (MenuType)Convert.ToInt32(modelMenu.MenuType);
                modelMenu.MenuCode = await GetMenuCodeFromMenu(modelMenu.MenuType, modelMenu.SubId, modelMenu.SubGroupId);

                foreach (MultiLingualProperty item in modelMenu.MenuTitles)
                {
                    Language lan = _controllerHelper.FetchLanguage(item.LanguageId);
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                    item.UrlFriend = await GetUrlFriend(modelMenu.MenuType, modelMenu.SubId, modelMenu.SubGroupId, item.LanguageId, cancellationToken);
                }

                modelMenu.Url = modelMenu.MenuType switch
                                {
                                    MenuType.ProductGroup => $"/group/{modelMenu.MenuCode}",
                                    MenuType.Product => $"/product/{modelMenu.MenuCode}",
                                    MenuType.CategoryContent => $"/category/{modelMenu.MenuCode}",
                                    MenuType.Content => $"/blog/{modelMenu.MenuCode}",
                                    _ => modelMenu.Url
                                };
                saveResult = await _menuRepository.UpdateAsync(modelMenu, cancellationToken);

                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    ActionTypes = ActionTypes.Update,
                                                    CollectionType = CollectionType.Menu,
                                                    Ip = _controllerHelper.GetUserIpAddress(),
                                                    ModifierId = userDb.Id,
                                                    ModifierUserName = userDb.UserName,
                                                    ModifyDateTime = DateTime.Now,
                                                    RecordId = modelMenu.Id
                                                };
                    Result<Modification> modifyInsert = await _modificationRepository.InsertAsync(modification, cancellationToken);

                    _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},editing menu with {dto.Id} id and {dto.MenuTitle} done successfully");
                    saveResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    saveResult.Message = ConstMessages.ErrorInSaving;
                }
            }
        }
        catch (Exception e)
        {
            saveResult.Succeeded = false;
            saveResult.Message = ConstMessages.ExceptionOccured;
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(MenuController)}/{nameof(Edit)}");
        }

        result = Json(saveResult.Succeeded
                          ? new { Status = "Success", saveResult.Message }
                          : new { Status = "Error", saveResult.Message });

        return result;
    }

    private async ValueTask<string> GetUrlFriend(MenuType type, string subId, string subGroupId, string lanId, CancellationToken cancellationToken)
    {
        string res = "";

        switch (type)
        {
            case MenuType.ProductGroup:
                ProductGroup grp = await _productGroupRepository.FirstOrDefaultAsync(c => c.Id == subGroupId, cancellationToken);
                res = grp.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? grp.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.UrlFriend : "";

                break;

            case MenuType.Product:
                DataLayer.Entities.Shop.Product.Product pro = await _productRepository.FirstOrDefaultAsync(c => c.Id == subId, cancellationToken);
                res = pro.MultiLingualProperties.Any(p => p.LanguageId == lanId) ? pro.MultiLingualProperties.FirstOrDefault(p => p.LanguageId == lanId)?.UrlFriend : "";

                break;

            case MenuType.CategoryContent:
                ContentCategory category = await _contentCategoryRepository.FirstOrDefaultAsync(c => c.Id == subGroupId, cancellationToken);
                res = category.CategoryNames.Any(p => p.LanguageId == lanId) ? category.CategoryNames.FirstOrDefault(p => p.LanguageId == lanId)?.UrlFriend : "";

                break;

            case MenuType.Content:
                DataLayer.Entities.General.Content.Content content = await _controllerHelper.ContentFetch(subId);
                res = content.LanguageId == lanId ? content.UrlFriend : "";

                break;

            case MenuType.DirectLink:
                break;

            case MenuType.Module:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return res;
    }

    private async ValueTask<long> GetMenuCodeFromMenu(MenuType type, string subId, string subGroupId)
    {
        long res = 0;

        switch (type)
        {
            case MenuType.ProductGroup:
                ProductGroup grp = _controllerHelper.ProductGroupFetch(subGroupId);
                res = grp.GroupCode;

                break;

            case MenuType.Product:
                ProductOutputDto pro = await _controllerHelper.ProductFetch(subId);
                res = pro.ProductCode;

                break;

            case MenuType.CategoryContent:
                ContentCategory category = await _controllerHelper.ContentCategoryFetch(subGroupId);
                res = category.CategoryCode;

                break;

            case MenuType.Content:
                DataLayer.Entities.General.Content.Content content = await _contentRepository.FirstOrDefaultAsync(c => c.ContentCategoryId == subGroupId);
                res = content.ContentCode;

                break;

            case MenuType.DirectLink:
                break;

            case MenuType.Module:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return res;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<Menu> opResult = new();
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            #region check object dependency
            //check whether it has any submenus or not
            bool allowDeletion = !(await _menuRepository.GetCountAsync(m => m.ParentId == id && m.IsActive && !m.IsDeleted, cancellationToken) > 0);
            #endregion

            Menu menu = await _menuRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (menu == null)
            {
                opResult.Message = ConstMessages.ObjectNotFound;
            }
            else
            {
                if (allowDeletion)
                {
                    opResult = await _menuRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, true, cancellationToken);

                    if (opResult.Succeeded)
                    {
                        Modification modification = new()
                                                    {
                                                        Id = Guid.NewGuid().ToString(),
                                                        ActionTypes = ActionTypes.Delete,
                                                        CollectionType = CollectionType.Menu,
                                                        Ip = _controllerHelper.GetUserIpAddress(),
                                                        ModifierId = userDb.Id,
                                                        ModifierUserName = userDb.UserName,
                                                        ModifyDateTime = DateTime.Now,
                                                        RecordId = menu.Id
                                                    };
                        Result<Modification> modifyInsert = await _modificationRepository.InsertAsync(modification, cancellationToken);

                        _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, deleting menu with {id} id and {menu.MenuTitles} done successfully");
                        opResult.Message = ConstMessages.SuccessfullyDone;
                    }
                    else
                    {
                        opResult.Message = ConstMessages.ErrorInSaving;
                    }
                }
            }
        }
        catch (Exception e)
        {
            opResult.Message = ConstMessages.ExceptionOccured;
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(MenuController)}/{nameof(Delete)}");
        }

        return Json(opResult.Succeeded
                        ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        Result<Menu> opResult = new();
        ApplicationUser userDb = await _controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            Menu menu = await _menuRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (menu == null)
            {
                opResult.Message = ConstMessages.ObjectNotFound;
            }
            else
            {
                opResult = await _menuRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);

                if (opResult.Succeeded)
                {
                    Modification modification = new()
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    ActionTypes = ActionTypes.Restore,
                                                    CollectionType = CollectionType.Menu,
                                                    Ip = _controllerHelper.GetUserIpAddress(),
                                                    ModifierId = userDb.Id,
                                                    ModifierUserName = userDb.UserName,
                                                    ModifyDateTime = DateTime.Now,
                                                    RecordId = menu.Id
                                                };
                    Result<Modification> modifyInsert = await _modificationRepository.InsertAsync(modification, cancellationToken);

                    _logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},restoring menu with {menu.Id} id and {menu.MenuTitles} done successfully");
                    opResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    opResult.Message = ConstMessages.ErrorInSaving;
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(MenuController)}/{nameof(Restore)}");
        }

        return Json(opResult.Succeeded
                        ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }
}