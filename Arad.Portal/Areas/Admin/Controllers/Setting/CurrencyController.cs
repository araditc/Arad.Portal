using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Currency;
using Arad.Portal.DataLayer.Models.Shared;
using AutoMapper;
using Arad.Portal.DataLayer.Entities.General.Currency;
using System.Threading;
using System.Collections.Specialized;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;

using Serilog;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Currency;
using Arad.Portal.Helpers.Shared;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class CurrencyController(
    ICurrencyRepository currencyRepository,
    IModificationRepository modificationRepository,
    IMapper mapper,
    ControllerHelper controllerHelper,
    ILogger logger)
    : Controller
{
    [HttpGet]
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        PagedItems<CurrencyDto> result = new();
        try
        {
            NameValueCollection filter = HttpUtility.ParseQueryString(Request.QueryString.ToString());

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
            string keyToFilter = "";
            if (!string.IsNullOrWhiteSpace(filter["filter"]))
            {
                keyToFilter = filter["filter"];
            }
            long totalCount = currencyRepository.GetCount(c => keyToFilter == "" || c.CurrencyName.Contains(keyToFilter));
            List<CurrencyDto> list = currencyRepository.GetList(c => keyToFilter == "" || c.CurrencyName.Contains(keyToFilter)).Skip((page - 1) * pageSize)
                                                        .Take(pageSize).Select(c => new CurrencyDto()
                                                                                    {
                                                                                        Id = c.Id,
                                                                                        IsDefault = c.IsDefault,
                                                                                        CurrencyName = c.CurrencyName,
                                                                                        Prefix = c.Prefix,
                                                                                        Symbol = c.Symbol,
                                                                                        IsDeleted = c.IsDeleted

                                                                                    }).ToList();


            result.CurrentPage = page;
            result.Items = list;
            result.ItemsCount = totalCount;
            result.PageSize = pageSize;
            result.QueryString = Request.QueryString.ToString();

        }
        catch (Exception e)
        {
            result.CurrentPage = 1;
            result.Items = new();
            result.ItemsCount = 0;
            result.PageSize = 10;
            result.QueryString = Request.QueryString.ToString();
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(CurrencyController)}/{nameof(List)}");

        }
        return View(result);
    }

    public async ValueTask<IActionResult> AddEdit(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Currency model = new();
            if (!string.IsNullOrEmpty(id))
            {
                model = controllerHelper.FetchCurrency(id).ReturnValue;
            }
            CurrencyDto dto = mapper.Map<CurrencyDto>(model);
            return View(dto);
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(CurrencyController)}/{nameof(AddEdit)}");
        }
        return View();
    }

    [HttpPost]
    public async ValueTask<IActionResult> Save([FromBody] CurrencyDto dto, CancellationToken cancellationToken)
    {
        Result<Currency> saveResult = new();
        JsonResult result;
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
                result = Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {
                Currency model = mapper.Map<Currency>(dto);
                model.IsDeleted = false;
                model.CreatorUserId = controllerHelper.GetCurrentUserId();
                model.CreatorUserName = controllerHelper.GetCurrentUser(cancellationToken).Result.UserName;
                model.CreationDate = DateTime.Now;
                saveResult = await currencyRepository.InsertAsync(model, cancellationToken);
                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    ActionTypes = ActionTypes.Insert,
                                                    CollectionType = CollectionType.Currency,
                                                    Ip = controllerHelper.GetUserIpAddress(),
                                                    ModifierId = userDb.Id,
                                                    ModifierUserName = userDb.UserName,
                                                    ModifyDateTime = DateTime.Now,
                                                    RecordId = model.Id
                                                };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding currency with {dto.Id} id and {dto.CurrencyName} done successfully");
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
            saveResult.Message = ConstMessages.ExceptionOccured;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(CurrencyController)}/{nameof(Save)}");
        }
        result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                          : new { Status = "Error", saveResult.Message });
        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<Currency> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Currency currency = await currencyRepository.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (currency == null)
            {
                opResult.Message = ConstMessages.ObjectNotFound;
            }
            else
            {
                opResult = await currencyRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, true, cancellationToken);

                Modification modification = new()
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                ActionTypes = ActionTypes.Delete,
                                                CollectionType = CollectionType.Currency,
                                                Ip = controllerHelper.GetUserIpAddress(),
                                                ModifierId = userDb.Id,
                                                ModifierUserName = userDb.UserName,
                                                ModifyDateTime = DateTime.Now,
                                                RecordId = currency.Id
                                            };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},deleting currency with {id} id and {currency.CurrencyName} done successfully");
                opResult.Succeeded = true;
                opResult.Message = ConstMessages.SuccessfullyDone;
            }
        }
        catch (Exception e)
        {
            opResult.Succeeded = false;
            opResult.Message = ConstMessages.ErrorInSaving;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(CurrencyController)}/{nameof(Delete)}");
        }

        return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        Result<Currency> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            Currency currency = await currencyRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (currency == null)
            {
                opResult.Message = ConstMessages.ObjectNotFound;
            }
            else
            {
                opResult = await currencyRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);

                Modification modification = new()
                                            {
                                                Id = Guid.NewGuid().ToString(),
                                                ActionTypes = ActionTypes.Restore,
                                                CollectionType = CollectionType.Currency,
                                                Ip = controllerHelper.GetUserIpAddress(),
                                                ModifierId = userDb.Id,
                                                ModifierUserName = userDb.UserName,
                                                ModifyDateTime = DateTime.Now,
                                                RecordId = currency.Id
                                            };
                Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, restoring currency with {id} id and {currency.CurrencyName} done successfully");
                opResult.Succeeded = true;
                opResult.Message = ConstMessages.SuccessfullyDone;
            }
        }
        catch (Exception e)
        {
            opResult.Succeeded = false;
            opResult.Message = ConstMessages.ExceptionOccured;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(CurrencyController)}/{nameof(Restore)}");
        }

        return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }
}