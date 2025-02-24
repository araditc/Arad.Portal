using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Entities.Shop.Setting;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.BasicData;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Currency;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.Shop.Setting;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.Currency;
using Arad.Portal.Models.Shared.Domain;
using Arad.Portal.Models.Shared.Setting;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.Currency;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;

using Microsoft.AspNetCore.Mvc.ModelBinding;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class ShippingController(
    IShippingSettingRepository shippingSettingRepository,
    IDomainRepository domainRepository,
    ICurrencyRepository currencyRepository,
    IModificationRepository modificationRepository,
    IBasicDataRepository basicDataRepository,
    ILogger logger,
    IMapper mapper,
    ControllerHelper controllerHelper)
    : Controller
{

    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        string queryString = Request.QueryString.ToString();
        PagedItems<ShippingSettingDto> result = new();

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

            int page = Convert.ToInt32(filter["page"]);
            int pageSize = Convert.ToInt32(filter["PageSize"]);

            long totalCount;
            List<ShippingSettingDto> list;

            if (userDb.IsSystemAccount)
            {
                totalCount = await shippingSettingRepository.GetCountAsync(c => true, cancellationToken);
                List<ShippingSetting> lst = (await shippingSettingRepository.GetAllAsync(cancellationToken)).Skip((page - 1) * pageSize)
                                                                                                            .Take(pageSize).ToList();

                list = mapper.Map<List<ShippingSettingDto>>(lst);
            }
            else
            {
                totalCount = await shippingSettingRepository.GetCountAsync(s => s.AssociatedDomainId == userDb.Domains.FirstOrDefault(a => a.IsOwner).DomainId, cancellationToken);
                List<ShippingSetting> lst = (await shippingSettingRepository.GetAllAsync(cancellationToken))
                                                                      .Where(s => s.AssociatedDomainId == userDb.Domains.FirstOrDefault(a => a.IsOwner)?.DomainId).Skip((page - 1) * pageSize)
                                                                      .Take(pageSize).ToList();

                list = mapper.Map<List<ShippingSettingDto>>(lst);
                foreach (ShippingSettingDto item in list)
                {
                    item.CurrencySymbol = (await currencyRepository.FirstOrDefaultAsync(c => c.Id == item.CurrencyId, cancellationToken)).Symbol;
                }
            }

            result.CurrentPage = page;
            result.Items = list;
            result.ItemsCount = totalCount;
            result.PageSize = pageSize;
            result.QueryString = queryString;


            foreach (ShippingSettingDto item in result.Items)
            {
                Result<DomainDto> resultDomainDto = new();
                Domain dbEntity = await domainRepository.FirstOrDefaultAsync(d => d.Id == item.AssociatedDomainId, cancellationToken);
                if (dbEntity == null)
                {
                    resultDomainDto.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
                }
                else
                {
                    DomainDto dto = mapper.Map<DomainDto>(dbEntity);
                    dto.SupportedLangId = (await basicDataRepository.GetAllAsync(cancellationToken))
                                                              .Where(d => d.AssociatedDomainId == dbEntity.Id && d.GroupKey == "SupportedCultures").Select(d => new string(d.Value)).ToList();
                    resultDomainDto.Succeeded = true;
                    resultDomainDto.Message = ConstMessages.SuccessfullyDone;
                    resultDomainDto.ReturnValue = dto;
                }
                item.DomainName = resultDomainDto.ReturnValue.DomainName;
            }
        }
        catch (Exception e)
        {
            result.CurrentPage = 1;
            result.Items = [];
            result.ItemsCount = 0;
            result.PageSize = 10;
            result.QueryString = queryString;
            result = new();
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ShippingController)}/{nameof(List)}");
        }
        return View(result);
    }

    public async ValueTask<IActionResult> AddEdit(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            ShippingSetting shippingSettingModel = new();
            UserDomain ownerDomain = userDb.Domains.FirstOrDefault(d => d.IsOwner);
            if (ownerDomain != null)
            {
                Result<Domain> domainEntity = controllerHelper.FetchDomain(userDb.Domains.FirstOrDefault(d => d.IsOwner)?.DomainId);
                if (domainEntity != null)
                {
                    shippingSettingModel.CurrencyId = domainEntity.ReturnValue.DefaultCurrencyId;
                    Currency defCurrency = controllerHelper.FetchCurrency(domainEntity.ReturnValue.DefaultCurrencyId).ReturnValue;
                    shippingSettingModel.CurrencySymbol = defCurrency.Symbol;
                    shippingSettingModel.AssociatedDomainId = domainEntity.ReturnValue.Id;
                }
            }


            ViewBag.IsSysAcc = userDb.IsSystemAccount;

            if (userDb.IsSystemAccount)
            {
                ViewBag.Domains = controllerHelper.GetAllActiveDomains();
            }

            ShippingSettingDto shippingSettingDto = mapper.Map<ShippingSettingDto>(shippingSettingModel);
            if (!string.IsNullOrEmpty(id))
            {
                ShippingSetting entity = await shippingSettingRepository.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

                if (entity.ShippingCoupon != null)
                {
                    shippingSettingDto.ShippingCoupon.PersianStartDate = entity.ShippingCoupon.StartDate.ToPersianDdate();
                    if (entity.ShippingCoupon.EndDate != null)
                    {
                        shippingSettingDto.ShippingCoupon.PersianEndDate = entity.ShippingCoupon.EndDate.Value.ToPersianDdate();
                    }
                }
            }

            ViewBag.ShippngTypes = controllerHelper.GetBasicDataList("ShippingType", true, true, cancellationToken);

            ViewBag.CurrencyList = controllerHelper.GetAllActiveCurrency();

            return View(shippingSettingDto);
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ShippingController)}/{nameof(AddEdit)}");
        }
        return View();
    }

    [HttpGet]
    public async ValueTask<IActionResult> GetSymbolOfCurrency(string currencyId, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Result<CurrencyDto> currencyEntity = new();
        try
        {
            Currency dbEntity = await currencyRepository.FirstOrDefaultAsync(c => c.Id == currencyId, cancellationToken);
            if (dbEntity == null)
            {
                currencyEntity.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
            }

            CurrencyDto dto = mapper.Map<CurrencyDto>(dbEntity);
            currencyEntity.Succeeded = true;
            currencyEntity.Message = ConstMessages.SuccessfullyDone;
            currencyEntity.ReturnValue = dto;

        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ShippingController)}/{nameof(GetSymbolOfCurrency)}");
            currencyEntity.Message = ConstMessages.ExceptionOccured;
        }

        return Json(new { symbol = currencyEntity.ReturnValue.Symbol });
    }

    [HttpPost]
    public async ValueTask<IActionResult> Add([FromBody] ShippingSettingDto dto, CancellationToken cancellationToken)
    {
        JsonResult result;
        Result<ShippingSetting> saveResult = new();
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
                if (!string.IsNullOrWhiteSpace(dto.ShippingCoupon.PersianStartDate))
                {
                    dto.ShippingCoupon.StartDate = dto.ShippingCoupon.PersianStartDate.Split(" ")[0].ToEnglishDate();
                }
                if (!string.IsNullOrWhiteSpace(dto.ShippingCoupon.PersianEndDate))
                {
                    dto.ShippingCoupon.EndDate = dto.ShippingCoupon.PersianEndDate.Split(" ")[0].ToEnglishDate();
                }
                dto.Id = Guid.NewGuid().ToString();
                string domainName = controllerHelper.GetCurrentDomainName();

                Result<CurrencyDto> resultCurrencyDto = new();
                try
                {
                    Currency dbEntity = await currencyRepository.FirstOrDefaultAsync(c => c.Id == dto.CurrencyId, cancellationToken);
                    if (dbEntity == null)
                    {
                        resultCurrencyDto.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
                    }

                    CurrencyDto currencyDto = mapper.Map<CurrencyDto>(dbEntity);
                    resultCurrencyDto.Succeeded = true;
                    resultCurrencyDto.Message = ConstMessages.SuccessfullyDone;
                    resultCurrencyDto.ReturnValue = currencyDto;

                }
                catch (Exception)
                {
                    resultCurrencyDto.Message = ConstMessages.ExceptionOccured;
                }
                dto.CurrencySymbol = resultCurrencyDto.ReturnValue.Symbol;
                ShippingSetting model = mapper.Map<ShippingSetting>(dto);
                model.CreatorUserName = userDb.UserName;
                model.CreatorUserId = userDb.Id;
                model.CreationDate = DateTime.Now;
                model.IsDeleted = false;
                model.IsActive = true;
                saveResult = await shippingSettingRepository.InsertAsync(model, cancellationToken);
                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Insert,
                        CollectionType = CollectionType.Shipping,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = model.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},adding shipping with {dto.Id} id done successfully");
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
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ShippingController)}/{nameof(Add)}");
            saveResult.Message = ConstMessages.ExceptionOccured;
            saveResult.Succeeded = false;
        }
        result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                          : new { Status = "Error", saveResult.Message });
        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Edit([FromBody] ShippingSettingDto dto, CancellationToken cancellationToken)
    {
        JsonResult result;
        Result<ShippingSetting> saveResult = new();
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
                        errors.AddRange(modelStateVal.Errors.Select(error =>
                                                                        new AjaxValidationErrorModel { Key = modelStateKey, ErrorMessage = error.ErrorMessage }));
                    }
                }

                return Json(new { Status = "ModelError", ModelStateErrors = errors });
            }
            else
            {

                ShippingSetting shipping = await shippingSettingRepository.FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

                if (shipping == null)
                {
                    return Json(new { Status = "ModelError" });
                }

                if (!string.IsNullOrWhiteSpace(dto.ShippingCoupon.PersianStartDate))
                {
                    dto.ShippingCoupon.StartDate = dto.ShippingCoupon.PersianStartDate.Split(" ")[0].ToEnglishDate();
                }

                if (!string.IsNullOrWhiteSpace(dto.ShippingCoupon.PersianEndDate))
                {
                    dto.ShippingCoupon.EndDate = dto.ShippingCoupon.PersianEndDate.Split(" ")[0].ToEnglishDate();
                }

                ShippingSetting shippingSetting = await shippingSettingRepository.FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

                if (shippingSetting.ShippingCoupon != null)
                {
                    dto.ShippingCoupon.PersianStartDate = shippingSetting.ShippingCoupon.StartDate.ToPersianDdate();

                    if (shippingSetting.ShippingCoupon.EndDate != null)
                    {
                        dto.ShippingCoupon.PersianEndDate = shippingSetting.ShippingCoupon.EndDate.Value.ToPersianDdate();
                    }
                }

                ShippingSetting model = mapper.Map(dto, shipping);
                saveResult = await shippingSettingRepository.UpdateAsync(model, cancellationToken);

                if (saveResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Update,
                        CollectionType = CollectionType.Shipping,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = model.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    saveResult.Message = ConstMessages.SuccessfullyDone;
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName},editing shipping with {dto.Id} id done successfully");
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
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ShippingController)}/{nameof(Edit)}");
        }
        result = Json(saveResult.Succeeded ? new { Status = "Success", saveResult.Message }
                          : new { Status = "Error", saveResult.Message });
        return result;
    }

    [HttpGet]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<ShippingSetting> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            ShippingSetting shipping = await shippingSettingRepository.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (shipping == null)
            {
                opResult.Message = ConstMessages.ObjectNotFound;
            }
            else
            {
                opResult = await shippingSettingRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, true, cancellationToken);
                if (opResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Delete,
                        CollectionType = CollectionType.Shipping,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = shipping.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, deleting shipping with {shipping.Id} id done successfully");
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
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ShippingController)}/{nameof(Delete)}");
            opResult.Succeeded = false;
            opResult.Message = ConstMessages.ExceptionOccured;
        }

        return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }

    [HttpGet]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        Result<Domain> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        try
        {
            ShippingSetting shipping = await shippingSettingRepository.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (shipping == null)
            {
                opResult.Message = ConstMessages.ObjectNotFound;
            }
            opResult = await domainRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);
            if (opResult.Succeeded)
            {
                if (shipping != null)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Restore,
                        CollectionType = CollectionType.Shipping,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = shipping.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);


                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, restoring shipping with {shipping.Id} id and done successfully");
                    opResult.Message = ConstMessages.SuccessfullyDone;
                }
            }
            else
            {
                opResult.Message = ConstMessages.ErrorInSaving;
            }
        }
        catch (Exception e)
        {
            opResult.Message = ConstMessages.ExceptionOccured;
            opResult.Succeeded = false;
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(ShippingController)}/{nameof(Restore)}");
        }

        return Json(opResult.Succeeded ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }


}