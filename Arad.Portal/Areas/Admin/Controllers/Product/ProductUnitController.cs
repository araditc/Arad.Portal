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
using Arad.Portal.DataLayer.Entities.Shop.ProductUnit;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Product;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.ProductUnit;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Product;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Arad.Portal.Areas.Admin.Controllers.Product;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class ProductUnitController(
    IProductUnitRepository unitRepository,
    IProductRepository productRepository,
    IModificationRepository modificationRepository,
    IMapper mapper,
    ControllerHelper controllerHelper,
    ILogger logger)
    : Controller
{
    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        string queryString = Request.QueryString.ToString();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Language lan = controllerHelper.GetDefaultLanguage();
        ViewBag.LangId = lan.Id;
        ViewBag.IsSystemAccount = userDb.IsSystemAccount;
        ViewBag.LangList = controllerHelper.GetAllActiveLanguage();
        ViewBag.Domains = controllerHelper.GetAllActiveDomains();

        PagedItems<ProductUnitViewModel> result = new();

        try
        {
            NameValueCollection filter = HttpUtility.ParseQueryString(queryString);

            if (string.IsNullOrWhiteSpace(filter["page"]))
            {
                filter.Set("page", "1");
            }

            if (string.IsNullOrWhiteSpace(filter["PageSize"]))
            {
                filter.Set("PageSize", "10");
            }

            if (string.IsNullOrWhiteSpace(filter["LanguageId"]))
            {
                filter.Set("LanguageId", lan.Id);
            }


            if (string.IsNullOrWhiteSpace(filter["Name"]))
            {
                filter.Set("Name", "");
            }

            string langId = filter["LanguageId"] ?? string.Empty;
            int page = Convert.ToInt32(filter["page"]);
            int pageSize = Convert.ToInt32(filter["PageSize"]);
            string filterName = filter["Name"] ?? string.Empty;
            long totalCount;
            string domainId = "";

            if (userDb.IsSystemAccount)
            {
                totalCount = await unitRepository.GetCountAsync(c => true, cancellationToken);
            }
            else
            {
                domainId = userDb.Domains.FirstOrDefault(c => c.IsOwner)?.DomainId;
                totalCount = await unitRepository.GetCountAsync(c => c.AssociatedDomainId == domainId, cancellationToken);
            }

            List<ProductUnitViewModel> list = (await unitRepository.GetListAsync(u =>
                    u.UnitNames.Any(a => filterName != null && a.Name.Contains(filterName))
                    && (string.IsNullOrEmpty(domainId) || u.AssociatedDomainId == domainId),
                                                                                 cancellationToken))
                .Skip((page - 1) * pageSize)
                .OrderByDescending(u => u.CreationDate)
                .Take(pageSize)
                .Select(u => new ProductUnitViewModel
                {
                    ProductUnitId = u.Id,
                    UnitName = u.UnitNames.Any(a => a.LanguageId == langId) ?
                                   u.UnitNames.First(a => a.LanguageId == langId) : u.UnitNames.First(),
                    IsDeleted = u.IsDeleted
                }).ToList();

            result.CurrentPage = page;
            result.Items = list;
            result.ItemsCount = totalCount;
            result.PageSize = pageSize;
            result.QueryString = queryString;
        }
        catch (Exception e)
        {
            result.CurrentPage = 1;
            result.Items = [];
            result.ItemsCount = 0;
            result.PageSize = 10;
            result.QueryString = queryString;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductUnitController)}/{nameof(List)}");
        }

        return View(result);
    }


    [HttpGet]
    public async ValueTask<IActionResult> AddEdit(string id, CancellationToken cancellationToken)
    {
        ProductUnitDto model = new();
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
            ProductUnit productModel = await unitRepository.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

            if (productModel != null)
            {
                model = mapper.Map<ProductUnitDto>(productModel);
            }
        }

        ViewBag.DefLang = controllerHelper.GetDefaultLanguage().Id;
        ViewBag.LangList = controllerHelper.GetAllActiveLanguage();

        return View(model);
    }

    [HttpPost]
    public async ValueTask<IActionResult> Add([FromBody] ProductUnitDto dto, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Result<ProductUnit> saveResult = new();

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

                return Json(new { Status = "ModelError", ModelStateErrors = errors });
            }

            ProductUnit model = mapper.Map<ProductUnit>(dto);

            foreach (MultiLingualProperty item in model.UnitNames)
            {
                Language lan = controllerHelper.FetchLanguage(item.LanguageId);
                item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                item.LanguageName = lan.LanguageName;
                item.LanguageSymbol = lan.Symbol;
            }

            model.CreationDate = DateTime.Now;
            model.CreatorUserId = userDb.Id;
            model.CreatorUserName = userDb.UserName;
            model.Id = Guid.NewGuid().ToString();
            model.IsActive = true;

            saveResult = await unitRepository.InsertAsync(model, cancellationToken);

            saveResult.Succeeded = true;

            if (saveResult.Succeeded)
            {
                Modification modification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    ActionTypes = ActionTypes.Insert,
                    CollectionType = CollectionType.ProductUnit,
                    Ip = controllerHelper.GetUserIpAddress(),
                    ModifierId = userDb.Id,
                    ModifierUserName = userDb.UserName,
                    ModifyDateTime = DateTime.Now,
                    RecordId = model.Id
                };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, adding product unit with {dto.Id} id and {dto.UnitNames.Select(c => c.Name)} done successfully");
                saveResult.Message = ConstMessages.SuccessfullyDone;

                return Json(new { Status = "Success", saveResult.Message });
            }

            saveResult.Message = ConstMessages.ErrorInSaving;

            return Json(new { Status = "Error", saveResult.Message });
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductUnitController)}/{nameof(Add)}");
        }

        return Json(saveResult.Succeeded
                        ? new { Status = "Success", saveResult.Message }
                        : new { Status = "Error", saveResult.Message });
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            ProductUnit productUnit = await unitRepository.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (productUnit == null)
            {
                result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_EntityNotFound") });
            }
            else
            {
                Result<ProductUnit> res = await unitRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);

                if (res.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Restore,
                        CollectionType = CollectionType.ProductUnit,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = productUnit.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},restoring product unit with {id} id and {productUnit.UnitNames.Select(c => c.Name)} done successfully");
                    res.Message = ConstMessages.SuccessfullyDone;
                    result = new(new { Status = "success", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully") });
                }
                else
                {
                    res.Message = ConstMessages.ErrorInSaving;
                    result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_TryLater") });
                }
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductUnitController)}/{nameof(Restore)}");
            result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_TryLater") });
        }

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Edit([FromBody] ProductUnitDto dto, CancellationToken cancellationToken)
    {
        Result<ProductUnit> saveResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            ProductUnit model = await unitRepository.FirstOrDefaultAsync(u => u.Id == dto.Id, cancellationToken);

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

                return Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                if (model == null)
                {
                    return RedirectToAction("PageOrItemNotFound", "Account");
                }

                if (await productRepository.AnyAsync(c => c.Unit.Id == model.Id, cancellationToken))
                {
                    return Json(new { Status = "ModelError", Message = ConstMessages.UpdatedNotAllowedForDependencies });
                }
                foreach (MultiLingualProperty item in dto.UnitNames)
                {
                    Language lan = controllerHelper.FetchLanguage(item.LanguageId);
                    item.MultiLingualPropertyId = Guid.NewGuid().ToString();
                    item.LanguageName = lan.LanguageName;
                    item.LanguageSymbol = lan.Symbol;
                }

                ProductUnit productUnit = mapper.Map(dto, model);

                productUnit.AssociatedDomainId = dto.AssociatedDomainId;
                saveResult = await unitRepository.UpdateAsync(productUnit, cancellationToken);

                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Update,
                        CollectionType = CollectionType.ProductUnit,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = productUnit.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},editing product unit with {dto.Id} id and {dto.UnitNames.Select(c => c.Name)} done successfully");
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
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductUnitController)}/{nameof(Edit)}");
            saveResult.Message = ConstMessages.ExceptionOccured;
        }

        JsonResult result = Json(saveResult.Succeeded
                                     ? new { Status = "Success", saveResult.Message }
                                     : new { Status = "Error", saveResult.Message });

        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<ProductUnit> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            #region check object dependency
            bool allowDeletion = true;

            //??? this part should be checked in mongo
            bool check = await productRepository.AnyAsync(c => c.Unit.Id == id, cancellationToken);

            if (check)
            {
                allowDeletion = false;
            }
            #endregion

            if (allowDeletion)
            {
                ProductUnit productUnit = await unitRepository.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

                if (productUnit != null)
                {
                    opResult = await unitRepository.UpdateAsync(u => u.Id == id, m => m.IsDeleted, true, cancellationToken);

                    if (opResult.Succeeded)
                    {
                        Modification modification = new()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActionTypes = ActionTypes.Delete,
                            CollectionType = CollectionType.ProductUnit,
                            Ip = controllerHelper.GetUserIpAddress(),
                            ModifierId = userDb.Id,
                            ModifierUserName = userDb.UserName,
                            ModifyDateTime = DateTime.Now,
                            RecordId = productUnit.Id
                        };
                        Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                        logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},deleting product unit with {id} id and {productUnit.UnitNames.Select(c => c.Name)} done successfully");
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
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ProductUnitController)}/{nameof(Delete)}");
            opResult.Message = ConstMessages.ExceptionOccured;
        }

        return Json(opResult.Succeeded
                        ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }

    [HttpGet]
    public async ValueTask<IActionResult> GetProductUnitList(string lid, string did)
    {
        JsonResult result;
        List<SelectListModel> lst = new();

        if (!string.IsNullOrWhiteSpace(did))
        {
            lst = await controllerHelper.GetAllActiveProductUnit(lid, did);
        }

        if (lst.Count > 0)
        {
            result = new(new { Status = "success", Data = lst.OrderBy(m => m.Text) });
        }
        else
        {
            result = new(new { Status = "error", Message = GeneralLibrary.Utilities.UtilityLanguage.GetString(ConstMessages.ObjectNotFound) });
        }

        return result;
    }
}