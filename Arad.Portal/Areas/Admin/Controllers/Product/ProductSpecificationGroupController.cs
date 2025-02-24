using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductSpecificationGroup;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.ProductSpecificationGroup;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Newtonsoft.Json;

using Serilog;

namespace Arad.Portal.Areas.Admin.Controllers.Product;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class ProductSpecificationGroupController(
    IProductSpecGroupRepository productSpecGroupRepository,
    ILogger logger,
    IMapper mapper,
    IModificationRepository modificationRepository,
    ControllerHelper controllerHelper,
    IProductSpecificationRepository productSpecificationRepository)
    : Controller
{
    private List<SpecificationGroupDto> _objects = [];

    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        PagedItems<SpecificationGroupViewModel> list = new();
        string queryString = Request.QueryString.ToString();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Language lan = controllerHelper.GetDefaultLanguage();

        try
        {
            NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

            if (string.IsNullOrWhiteSpace(filter["page"]))
            {
                filter.Set("page", "1");
            }

            if (string.IsNullOrWhiteSpace(filter["PageSize"]))
            {
                filter.Set("PageSize", "20");
            }

            if (string.IsNullOrWhiteSpace(filter["LanguageId"]))
            {
                filter.Set("LanguageId", lan.Id);
            }

            if (string.IsNullOrWhiteSpace(filter["Name"]))
            {
                filter.Set("Name", "");
            }

            string langId = filter["LanguageId"];
            int page = Convert.ToInt32(filter["page"]);
            int pageSize = Convert.ToInt32(filter["PageSize"]);
            string filterName = filter["Name"];
            long totalCount;
            string? domainId = "";

            if (userDb.IsSystemAccount)
            {
                totalCount = await productSpecGroupRepository.GetCountAsync(c => true, cancellationToken);
            }
            else
            {
                domainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
                totalCount = await productSpecGroupRepository.GetCountAsync(g => g.AssociatedDomainId == domainId, cancellationToken);
            }

            List<SpecificationGroupViewModel> production = (await productSpecGroupRepository.GetAllAsync(cancellationToken)).AsQueryable()
                                                                                                                            .Where(a => a.GroupNames.Any(b => b.Name.Contains(filterName)) &&
                                                                                                                                        a.GroupNames.Any(multiLingualProperty => multiLingualProperty.LanguageId.ToString() == langId) &&
                                                                                                                                        (string.IsNullOrEmpty(domainId) || a.AssociatedDomainId == domainId))
                                                                                                                            .Skip((page - 1) * pageSize)
                                                                                                                            .OrderByDescending(g => g.CreationDate)
                                                                                                                            .Take(pageSize)
                                                                                                                            .Select(g => new SpecificationGroupViewModel
                                                                                                                            {
                                                                                                                                SpecificationGroupId = g.Id,
                                                                                                                                IsDeleted = g.IsDeleted,
                                                                                                                                GroupName = g.GroupNames.Any(a => a.LanguageId.ToString() == langId)
                                                                                                                                                             ? g.GroupNames.First(a => a.LanguageId.ToString() == langId)
                                                                                                                                                             : g.GroupNames.First()
                                                                                                                            })
                                                                                                                            .ToList();

            list.CurrentPage = page;
            list.Items = production;
            list.ItemsCount = totalCount;
            list.PageSize = pageSize;
            list.QueryString = queryString;
            ViewBag.LangId = lan.Id;

            List<SelectListModel> languages = controllerHelper.GetAllActiveLanguage();
            languages.Insert(0, new() { Text = GeneralLibrary.Utilities.UtilityLanguage.GetString("Choose"), Value = "-1" });
            ViewBag.LangList = languages;
            ViewBag.Domains = controllerHelper.GetAllActiveDomains();
            ViewBag.IsSystemAccount = userDb.IsSystemAccount;
        }
        catch (Exception e)
        {
            list.CurrentPage = 1;
            list.Items = [];
            list.ItemsCount = 0;
            list.PageSize = 10;
            list.QueryString = queryString;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductSpecificationGroupController)}/{nameof(Restore)}");
        }

        return View(list);
    }

    public async ValueTask<IActionResult> AddEdit(string id, CancellationToken cancellationToken)
    {
        SpecificationGroupDto model = new();

        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        if (userDb.IsSystemAccount)
        {
            ViewBag.Domains = controllerHelper.GetAllActiveDomains();
        }
        else
        {
            model.AssociatedDomainId = userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId;
        }

        ViewBag.IsSysAcc = userDb.IsSystemAccount;

        if (!string.IsNullOrWhiteSpace(id))
        {
            Task<ProductSpecGroup> groupSpec = controllerHelper.GroupSpecificationFetch(id);
            model = mapper.Map<SpecificationGroupDto>(groupSpec.Result);
        }

        Language lan = controllerHelper.GetDefaultLanguage();
        ViewBag.LangId = lan.Id;

        ViewBag.LangList = controllerHelper.GetAllActiveLanguage();

        ViewBag.CurrencyList = controllerHelper.GetAllActiveCurrency();

        return View(model);
    }

    [HttpPost]
    public async ValueTask<IActionResult> CreateOne(SpecificationGroupDto dto, CancellationToken cancellationToken)
    {
        ApplicationUser user = await controllerHelper.GetCurrentUser(cancellationToken);

        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(AddEdit), user.Id);
        }

        _objects.Add(dto); // Add the object to the list

        // Optionally, you can display the objects in a temporary table
        TempData["Objects"] = _objects;

        return RedirectToAction(nameof(AddEdit), user.Id);
    }

    [HttpPost]
    public async ValueTask<ActionResult> SaveAll(CancellationToken cancellationToken)
    {
        if (TempData.ContainsKey("Objects"))
        {
            string serializedObjects = (string)TempData["Objects"];

            // Deserialize the objects from TempData
            _objects = JsonConvert.DeserializeObject<List<SpecificationGroupDto>>(serializedObjects);

            // Save all objects to the database
            List<ProductSpecGroup> objectsModel = mapper.Map<List<ProductSpecGroup>>(_objects);
            await productSpecGroupRepository.InsertAsync(objectsModel, cancellationToken);
            TempData.Remove("Objects");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async ValueTask<IActionResult> Add([FromBody] SpecificationGroupDto dto, CancellationToken cancellationToken)
    {
        Result<ProductSpecGroup> saveResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

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

                Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                foreach (MultiLingualProperty item in dto.GroupNames)
                {
                    Language lan = controllerHelper.FetchLanguage(item.LanguageId);
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                    Result<Currency> res = controllerHelper.FetchCurrency(item.CurrencyId);
                    item.CurrencyName = res.ReturnValue.CurrencyName;
                    item.CurrencyPrefix = res.ReturnValue.Prefix;
                    item.CurrencySymbol = res.ReturnValue.Symbol;
                }

                ProductSpecGroup model = mapper.Map<ProductSpecGroup>(dto);
                model.CreationDate = DateTime.Now;
                model.CreatorUserId = controllerHelper.GetCurrentUserId();
                model.CreatorUserName = controllerHelper.GetCurrentUser(cancellationToken).Result.UserName;
                model.Id = Guid.NewGuid().ToString();
                model.IsActive = true;
                saveResult = await productSpecGroupRepository.InsertAsync(model, cancellationToken);

                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Insert,
                        CollectionType = CollectionType.ProductSpecGroup,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = model.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding product specification group with {dto.Id} id and {dto.GroupNames.Select(c => c.Name)} done successfully");
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
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductSpecificationGroupController)}/{nameof(Add)}");
        }

        JsonResult result = Json(saveResult.Succeeded
                                     ? new { Status = "Success", saveResult.Message }
                                     : new { Status = "Error", saveResult.Message });

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        Result<ProductSpecGroup> res = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        JsonResult result;

        try
        {
            ProductSpecGroup productSpecGroup = await productSpecGroupRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (productSpecGroup == null)
            {
                res.Message = ConstMessages.ObjectNotFound;
            }

            res = await productSpecGroupRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);

            if (res.Succeeded)
            {
                if (productSpecGroup != null)
                {
                    Modification modification = new()
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    ActionTypes = ActionTypes.Restore,
                                                    CollectionType = CollectionType.ProductSpecGroup,
                                                    Ip = controllerHelper.GetUserIpAddress(),
                                                    ModifierId = userDb.Id,
                                                    ModifierUserName = userDb.UserName,
                                                    ModifyDateTime = DateTime.Now,
                                                    RecordId = productSpecGroup.Id
                                                };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
                }

                if (productSpecGroup != null)
                {
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},restoring product specification group with {id} id and {productSpecGroup.GroupNames.Select(c => c.Name)} done successfully");
                }

                res.Message = ConstMessages.SuccessfullyDone;
                result = new(new { Status = "success", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully") });
            }
            else
            {
                res.Message = ConstMessages.ErrorInSaving;
                result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_TryLater") });
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductSpecificationGroupController)}/{nameof(Restore)}");
            res.Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
            result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_TryLater") });
        }

        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="lid"></param>
    /// <param name="did"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async ValueTask<IActionResult> GetSpecificationGroupList([FromQuery] string lid, [FromQuery] string did, CancellationToken cancellationToken)
    {
        JsonResult result;
        List<SelectListModel> lst = [];
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            if (!string.IsNullOrEmpty(did))
            {
                lst = (await controllerHelper.AllActiveSpecificationGroup(lid, did, cancellationToken)).OrderBy(m => m.Text).ToList();
            }

            if (lst.Count > 0)
            {
                result = new(new { Status = "success", Data = lst });
                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},getting list of product specification group done successfully");
            }
            else
            {
                result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductSpecificationGroupController)}/{nameof(GetSpecificationGroupList)}");
            result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
        }

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Edit([FromBody] SpecificationGroupDto dto, CancellationToken cancellationToken)
    {
        Result<ProductSpecGroup> saveResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

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

                Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                ProductSpecGroup model = await controllerHelper.GroupSpecificationFetch(dto.Id);

                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }

                foreach (MultiLingualProperty item in dto.GroupNames)
                {
                    Language lan = controllerHelper.FetchLanguage(item.LanguageId);
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                    Result<Currency> res = controllerHelper.FetchCurrency(item.CurrencyId);
                    item.CurrencyName = res.ReturnValue.CurrencyName;
                    item.CurrencyPrefix = res.ReturnValue.Prefix;
                    item.CurrencySymbol = res.ReturnValue.Symbol;
                }

                ProductSpecGroup productSpecGrp = mapper.Map(dto , model);
                
                saveResult = await productSpecGroupRepository.UpdateAsync(productSpecGrp, cancellationToken);
                saveResult.Succeeded = true;

                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Update,
                        CollectionType = CollectionType.ProductSpecGroup,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = productSpecGrp.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    saveResult.Message = ConstMessages.SuccessfullyDone;
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},editing product specification group with {dto.Id} id and {dto.GroupNames.Select(c => c.Name)} done successfully");
                }
                else
                {
                    saveResult.Message = ConstMessages.ErrorInSaving;
                }
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductSpecificationGroupController)}/{nameof(Edit)}");
            saveResult.Succeeded = false;
            saveResult.Message = ConstMessages.ErrorInSaving;
        }

        JsonResult result = Json(saveResult.Succeeded
                                     ? new { Status = "Success", saveResult.Message }
                                     : new { Status = "Error", saveResult.Message });

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<ProductSpecGroup> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            #region check object dependency
            bool allowDeletion = true;

            //??? this part should be checked in mongo
            bool check = await productSpecificationRepository.AnyAsync(s => s.SpecificationGroupId == id, cancellationToken);

            if (check)
            {
                allowDeletion = false;
            }
            #endregion

            ProductSpecGroup productSpecGroup = await productSpecGroupRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (productSpecGroup == null)
            {
                opResult.Succeeded = false;
                opResult.Message = ConstMessages.ObjectNotFound;
            }
            else
            {
                if (allowDeletion)
                {
                    opResult = await productSpecGroupRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, true, cancellationToken);

                    if (opResult.Succeeded)
                    {
                        Modification modification = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActionTypes = ActionTypes.Delete,
                            CollectionType = CollectionType.ProductSpecGroup,
                            Ip = controllerHelper.GetUserIpAddress(),
                            ModifierId = userDb.Id,
                            ModifierUserName = userDb.UserName,
                            ModifyDateTime = DateTime.Now,
                            RecordId = productSpecGroup.Id
                        };
                        Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                        opResult.Message = ConstMessages.SuccessfullyDone;
                        logger.Information(
                            $"userId: {userDb.Id}, userName: {userDb.UserName},adding product specification group with {productSpecGroup.Id} id and {productSpecGroup.GroupNames.Select(c => c.Name)} done successfully");
                    }
                    else
                    {
                        opResult.Message = ConstMessages.GeneralError;
                    }
                }
                else
                {
                    opResult.Message = ConstMessages.DeletedNotAllowedForDependencies;
                }
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductSpecificationGroupController)}/{nameof(Delete)}");
            opResult.Message = ConstMessages.ExceptionOccured;
        }

        return Json(opResult.Succeeded
                        ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }
}