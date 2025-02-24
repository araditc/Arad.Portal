using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Entities.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductSpecification;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.ProductSpecification;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Serilog;

namespace Arad.Portal.Areas.Admin.Controllers.Product;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class ProductSpecificationController(
    IProductSpecificationRepository specificationRepository,
    IMapper mapper,
    ControllerHelper controllerHelper,
    ILogger logger,
    IModificationRepository modificationRepository,
    IProductRepository productRepository)
    : Controller
{
    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        string queryString = Request.QueryString.ToString();
        PagedItems<ProductSpecificationViewModel> result = new();
        NameValueCollection filter = HttpUtility.ParseQueryString(queryString);
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Language lan = controllerHelper.GetDefaultLanguage();

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

        int page = Convert.ToInt32(filter["page"]);
        int pageSize = Convert.ToInt32(filter["PageSize"]);
        string langId = filter["LanguageId"]!;
        string filterName = filter["Name"]!;
        long totalCount;
        string? domainId = "";

        if (userDb.IsSystemAccount)
        {
            totalCount = await specificationRepository.GetCountAsync(c => true, cancellationToken);
        }
        else
        {
            domainId = userDb.Domains.FirstOrDefault(c => c.IsOwner)?.DomainId;
            totalCount = await specificationRepository.GetCountAsync(c => c.AssociatedDomainId == domainId, cancellationToken);
        }

        List<ProductSpecificationViewModel> list = (await specificationRepository.GetAllAsync(cancellationToken))
                                                   .Where(c =>
                                                              c.SpecificationNameValues.Any(a => filterName != null && a.Name.Contains(filterName)) &&
                                                              c.SpecificationNameValues.Any(a => a.LanguageId == langId) &&
                                                              (domainId == "" || c.AssociatedDomainId == domainId))
                                                   .Skip((page - 1) * pageSize)
                                                   .OrderByDescending(c => c.CreationDate)
                                                   .Take(pageSize)
                                                   .Select(c => new ProductSpecificationViewModel
                                                   {
                                                       ProductSpecificationId = c.Id,
                                                       SpecificationGroupId = c.SpecificationGroupId,
                                                       SpecificationNameValues =
                                                                        c.SpecificationNameValues.Any(a => a.LanguageId == langId) ? c.SpecificationNameValues.First(a => a.LanguageId == langId) : c.SpecificationNameValues.First(),
                                                       IsDeleted = c.IsDeleted
                                                   })
                                                   .ToList();
        result.CurrentPage = page;
        result.ItemsCount = totalCount;
        result.Items = list;
        result.PageSize = pageSize;
        result.QueryString = queryString;

        try
        {
            ViewBag.LangId = lan.Id;

            List<SelectListModel> languages = controllerHelper.GetAllActiveLanguage();
            languages.Insert(0, new() { Text = GeneralLibrary.Utilities.UtilityLanguage.GetString("Choose"), Value = "-1" });
            ViewBag.LangList = languages;
            ViewBag.IsSystemAccount = userDb.IsSystemAccount;

            if (userDb.IsSystemAccount)
            {
                ViewBag.Domains = controllerHelper.GetAllActiveDomains();
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductSpecificationController)}/{nameof(List)}");
        }

        return View(result);
    }

    public async ValueTask<IActionResult> AddEdit(string id, CancellationToken cancellationToken)
    {
        ProductSpecification model = new();

        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        if (userDb.IsSystemAccount)
        {
            ViewBag.Domains = controllerHelper.GetAllActiveDomains();
        }

        model.AssociatedDomainId = userDb.Domains.FirstOrDefault(c => c.IsOwner)?.DomainId;

        ViewBag.IsSysAcc = userDb.IsSystemAccount;

        if (!string.IsNullOrWhiteSpace(id))
        {
            model = await specificationRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        Language lan = controllerHelper.GetDefaultLanguage();
        ViewBag.LangId = lan.Id;

        ViewBag.LangList = controllerHelper.GetAllActiveLanguage();

        List<SelectListModel> currencyList = controllerHelper.GetAllActiveCurrency();
        currencyList.Insert(0, new() { Value = "-1", Text = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_Choose") });
        ViewBag.CurrencyList = currencyList;

        List<SelectListModel> controlTypes = [];
        controlTypes.AddRange(from int i in Enum.GetValues(typeof(ControlType)) let name = Enum.GetName(typeof(ControlType), i)! select new SelectListModel() { Text = name, Value = i.ToString() });

        controlTypes.Insert(0, new() { Text = GeneralLibrary.Utilities.UtilityLanguage.GetString("Choose"), Value = "-1" });
        ViewBag.ControlTypes = controlTypes;

        List<SelectListModel> groupList = await controllerHelper.AllActiveSpecificationGroup(lan.Id, "", cancellationToken);
        groupList.Insert(0, new() { Text = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_Choose"), Value = "" });
        ViewBag.SpecificationGroupList = groupList;

        ProductSpecificationDto dto = mapper.Map<ProductSpecificationDto>(model);

        return View(dto);
    }

    [HttpGet]
    public async ValueTask<IActionResult> GetSpecificationInGroupAndLang(string groupId, string langId, string domainId, CancellationToken cancellationToken)
    {
        List<SelectListModel> lst;
        Task<ApplicationUser> userDb = controllerHelper.GetCurrentUser(cancellationToken);

        if (userDb.Result.IsSystemAccount && string.IsNullOrEmpty(domainId))
        {
            lst = (await specificationRepository.GetListAsync(c => c.Id == groupId && c.IsActive && !c.IsDeleted, cancellationToken))
                                         .Select(c => new SelectListModel
                                         {
                                             Text = c.SpecificationNameValues.Count(a => a.LanguageId == langId) != 0 ? c.SpecificationNameValues.FirstOrDefault(a => a.LanguageId == langId)?.Name : "",
                                             Value = c.Id.ToString()
                                         })
                                         .ToList();
        }
        else
        {
            string finalDomainId = !string.IsNullOrEmpty(domainId) ? domainId : userDb.Result.Domains.FirstOrDefault(a => a.IsOwner)?.DomainId;
            List<ProductSpecification> list = (await specificationRepository.GetListAsync(c => c.SpecificationGroupId == groupId &&
                                                                                               c.IsActive &&
                                                                                               !c.IsDeleted &&
                                                                                               c.AssociatedDomainId == finalDomainId,
                                                                                          cancellationToken))
                                                                     .ToList();

            lst = list
                  .Select(c => new SelectListModel { Text = c.SpecificationNameValues.Any(a => a.LanguageId == langId) ? c.SpecificationNameValues.FirstOrDefault(a => a.LanguageId == langId)?.Name : "", Value = c.Id.ToString() })
                  .ToList();
        }

        JsonResult result = lst.Count > 0 ? new(new { Status = "success", Data = lst }) : new JsonResult(new { Status = "error", message = GeneralLibrary.Utilities.UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> GetSpecificationValuesInLanguage(string specId, string langId, CancellationToken cancellationToken)
    {
        ProductSpecification res = await specificationRepository.FirstOrDefaultAsync(c => c.Id == specId, cancellationToken);

        List<SelectListModel> lst = res.SpecificationNameValues.FirstOrDefault(c => c.LanguageId == langId)
                                       ?.NameValues.Select(c => new SelectListModel { Text = c.Trim(), Value = "" })
                                       .OrderBy(c => c.Text)
                                       .ToList();
        JsonResult result = lst is { Count: > 0 } ? new(new { Status = "success", Data = lst }) : new JsonResult(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Add([FromBody] ProductSpecificationDto dto, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Result<ProductSpecification> saveResult = new();

        try
        {
            if (!ModelState.IsValid)
            {
                List<AjaxValidationErrorModel> errors = [];

                foreach (string modelStateKey in ModelState.Keys)
                {
                    ModelStateEntry modelStateVal = ModelState[modelStateKey]!;

                    errors.AddRange(modelStateVal.Errors
                                                 .Select(error => new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                }

                Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                ProductSpecGroup group = await controllerHelper.GroupSpecificationFetch(dto.SpecificationGroupId);

                foreach (MultiLingualProperty item in dto.SpecificationNameValues)
                {
                    Language lan = controllerHelper.FetchLanguage(item.LanguageId);

                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                    item.GroupName = group != null ? group.GroupNames.FirstOrDefault(c => c.LanguageId == lan.Id)?.Name : "";
                }

                ProductSpecification model = mapper.Map<ProductSpecification>(dto);
                model.Id = Guid.NewGuid().ToString();

                model.CreationDate = DateTime.Now;
                model.CreatorUserId = controllerHelper.GetCurrentUserId();
                model.CreatorUserName = controllerHelper.GetCurrentUser(cancellationToken).Result.UserName;
                model.IsActive = true;
                saveResult = await specificationRepository.InsertAsync(model, cancellationToken);

                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Insert,
                        CollectionType = CollectionType.ProductSpecification,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = model.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding product specification with {dto.Id} id done successfully");
                    saveResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    saveResult.Message = ConstMessages.InternalServerErrorMessage;
                }
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductSpecificationController)}/{nameof(Add)}");
        }

        JsonResult result = Json(saveResult.Succeeded
                                     ? new { Status = "Success", saveResult.Message }
                                     : new { Status = "Error", saveResult.Message });

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        JsonResult result;
        ProductSpecification productSpec = await controllerHelper.SpecificationFetch(id, cancellationToken);
        try
        {
            if (productSpec != null)
            {
                Result<ProductSpecification> res = await specificationRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);

                if (res.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Restore,
                        CollectionType = CollectionType.ProductSpecification,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = productSpec.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},restoring product specification with {id} id done successfully");
                    res.Message = ConstMessages.SuccessfullyDone;
                    result = new(new { Status = "success", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully") });
                }
                else
                {
                    res.Message = ConstMessages.ErrorInSaving;
                    result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_TryLater") });
                }
            }
            result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_TryLater") });
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductSpecificationController)}/{nameof(Restore)}");
            result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_TryLater") });
        }

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Edit([FromBody] ProductSpecificationDto dto, CancellationToken cancellationToken)
    {
        Result<ProductSpecification> saveResult = new();
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
                ProductSpecification model = await controllerHelper.SpecificationFetch(dto.Id, cancellationToken);

                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }

                ProductSpecGroup group = await controllerHelper.GroupSpecificationFetch(dto.SpecificationGroupId);

                foreach (MultiLingualProperty item in dto.SpecificationNameValues)
                {
                    Language lan = controllerHelper.FetchLanguage(item.LanguageId);
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                    item.GroupName = group?.GroupNames.FirstOrDefault(c => c.LanguageId == lan.Id) != null ? group.GroupNames.FirstOrDefault(c => c.LanguageId == lan.Id)?.Name : "";
                }

                ProductSpecification productSpecification = mapper.Map(dto, model);

                saveResult = await specificationRepository.UpdateAsync(productSpecification, cancellationToken);

                if (saveResult.Succeeded)
                {

                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Update,
                        CollectionType = CollectionType.ProductSpecification,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = productSpecification.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},editing product specification with {dto.Id} id done successfully");
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
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductSpecificationController)}/{nameof(Restore)}");
        }

        JsonResult result = Json(saveResult.Succeeded
                                     ? new { Status = "Success", saveResult.Message }
                                     : new { Status = "Error", saveResult.Message });

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<ProductSpecification> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            #region check object dependency
            bool allowDeletion = true;

            //??? this part should be checked in mongo
            bool check = await productRepository.AnyAsync(baseProduct => baseProduct.Specifications.Any(c => c.SpecificationId == id), cancellationToken);

            if (check)
            {
                allowDeletion = false;
            }
            #endregion

            if (allowDeletion)
            {
                ProductSpecification productSpecification = await specificationRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

                if (productSpecification != null)
                {
                    opResult = await specificationRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, true, cancellationToken);

                    if (opResult.Succeeded)
                    {

                        Modification modification = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActionTypes = ActionTypes.Delete,
                            CollectionType = CollectionType.ProductSpecification,
                            Ip = controllerHelper.GetUserIpAddress(),
                            ModifierId = userDb.Id,
                            ModifierUserName = userDb.UserName,
                            ModifyDateTime = DateTime.Now,
                            RecordId = productSpecification.Id
                        };
                        Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);
                        logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},deleting product specification with {id} id and {productSpecification.SpecificationNameValues.FirstOrDefault()?.Name} done successfully");
                        opResult.Message = ConstMessages.SuccessfullyDone;
                        opResult.Succeeded = true;
                    }
                    else
                    {
                        opResult.Message = ConstMessages.GeneralError;
                    }
                }
                else
                {
                    opResult.Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
                }
            }
            else
            {
                opResult.Message = ConstMessages.DeletedNotAllowedForDependencies;
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductSpecificationController)}/{nameof(Delete)}");
            opResult.Message = ConstMessages.ExceptionOccured;
        }

        return Json(opResult.Succeeded
                        ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }
}