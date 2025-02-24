using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.Modification;
using Arad.Portal.DataLayer.Entities.General.SendMessage;
using Arad.Portal.DataLayer.Entities.General.SMS;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Implementations.General.Domain.Mongo;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Modification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SendMessage;
using Arad.Portal.GeneralLibrary.Utilities;
using Arad.Portal.Helpers.Shared;
using Arad.Portal.Models.Shared;
using Arad.Portal.Models.Shared.SendMessage;

using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using Serilog;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Arad.Portal.Areas.Admin.Controllers.Setting;

[Authorize(Policy = "Role")]
[Area("Admin")]
public class SendMessageController(ControllerHelper controllerHelper, ISendMessageRepository sendMessageRepository, IModificationRepository modificationRepository, ILogger logger, IMapper mapper)
    : Controller
{
    public async ValueTask<IActionResult> List(CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        PagedItems<SendMessageViewModel> result = new();

        try
        {
            NameValueCollection filter = HttpUtility.ParseQueryString(Request.QueryString.ToString());

            filter.Set("page", "1");
            filter.Set("PageSize", "20");

            int page = Convert.ToInt32(filter["page"]);
            int pageSize = Convert.ToInt32(filter["PageSize"]);
            string filterKey = filter["filter"] ?? "";

            string domainId = userDb.IsSystemAccount
                                ? ""
                                : userDb.Domains.FirstOrDefault(c => c.IsOwner)?.DomainId;

            // Query for SendMessage entities
            long totalCount = await sendMessageRepository.GetCountAsync(c => string.IsNullOrEmpty(domainId) || c.AssociatedDomainId == domainId, cancellationToken);

            // Fetch the list of SendMessage
            List<SendMessage> lst = await sendMessageRepository.GetListAsync(c => string.IsNullOrEmpty(domainId) || c.AssociatedDomainId == domainId, cancellationToken);

            List<SendMessageViewModel> sendMessageViewModels = lst.Select(sm =>
            {
                // Initialize the view model
                SendMessageViewModel viewModel = new()
                                                 {
                                                     Id = sm.Id,
                                                     SendType = sm.SendType,
                                                     Provider = sm.Provider,
                                                     IsDeleted = sm.IsDeleted,
                                                     CreatingDate = sm.CreationDate
                                                 };

                // Deserialize metadata based on its type
                    if (sm.SendType == SendType.SMS)
                    {
                        Sms sms = JsonConvert.DeserializeObject<Sms>(sm.SendMessageMetaData.ToString() ?? string.Empty);
                        viewModel.BaseAddress = sms?.BaseAddress ?? string.Empty;
                        viewModel.SenderNumber = sms?.SenderNumber ?? string.Empty;
                        viewModel.ApiKey = sms?.ApiKey ?? string.Empty;
                    }
                    else if (sm.SendType == SendType.Email)
                    {
                        Smtp smtp = JsonConvert.DeserializeObject<Smtp>(sm.SendMessageMetaData.ToString() ?? string.Empty);
                        viewModel.Server = smtp?.Server ?? string.Empty;
                        viewModel.Port = smtp?.Port ?? string.Empty;
                        viewModel.UserName = smtp?.UserName ?? string.Empty;
                        viewModel.Password = smtp?.Password ?? string.Empty;
                    }
                    
                return viewModel;
            }).ToList();

            // Filter and paginate the result based on the filterKey
            sendMessageViewModels = sendMessageViewModels
                .Where(c => string.IsNullOrWhiteSpace(filterKey) || c.ApiKey.Contains(filterKey))
                .OrderByDescending(c => c.CreatingDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Populate result with the paginated data
            result.CurrentPage = page;
            result.Items = sendMessageViewModels;
            result.ItemsCount = totalCount;
            result.PageSize = pageSize;
            result.QueryString = Request.QueryString.ToString();
        }
        catch (Exception ex)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {ex.Message} occurred. stack trace: {nameof(SendMessageController)}/{nameof(List)}");
        }

        return View(result);
    }


    [HttpGet]
    public async Task<IActionResult> AddEditSms(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        if (userDb.IsSystemAccount)
        {
            ViewBag.Domains = controllerHelper.GetAllActiveDomains();
        }

        List<SelectListModel> lst = [];
        lst.AddRange(from int i in Enum.GetValues(typeof(Provider)) let name = Enum.GetName(typeof(Provider), i) select new SelectListModel { Text = name, Value = i.ToString() });

        lst.Insert(0, new() { Text = UtilityLanguage.GetString("Choose"), Value = "-1" });

        ViewBag.Providers = lst;
        ViewBag.IsSysAcc = userDb.IsSystemAccount;

        if (string.IsNullOrEmpty(id))
        {
            return View(new SmsDto());
        }

        SendMessage sendMessage = await sendMessageRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (sendMessage is not { SendType: SendType.SMS })
        {
            return NotFound();
        }

        Sms sms = JsonConvert.DeserializeObject<Sms>(sendMessage.SendMessageMetaData.ToString());
        SmsDto smsDto = mapper.Map<SmsDto>(sms);
        smsDto.Provider = sendMessage.Provider;
        smsDto.Id = sendMessage.Id;
        smsDto.AssociatedDomainId = sendMessage.AssociatedDomainId;

        return View(smsDto);
    }

    [HttpPost]
    public async Task<IActionResult> AddSms(SmsDto dto, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Domain domain = controllerHelper.GetCurrentUserDomain();
        // If the model is invalid, return the view with validation errors.
        if (!ModelState.IsValid)
        {
            // Populate necessary ViewBag properties (if any are needed)
            ViewBag.Domains = controllerHelper.GetAllActiveDomains();
            ViewBag.Providers = Enum.GetValues(typeof(Provider)).Cast<Provider>(); // Example for providers

            return View("AddEditSms", dto); // Return the view with the current dto and ModelState errors
        }

        // Map SmsDto to SendMessage entity
        SendMessage sendMessageEntity = new()
        {
            SendType = SendType.SMS,
            SendMessageMetaData = mapper.Map<Sms>(dto),
            CreationDate = DateTime.Now,
            IsDeleted = false,
            IsActive = true,
            CreatorUserId = userDb.Id,
            CreatorUserName = userDb.UserName,
            Id = Guid.NewGuid().ToString(),
            Provider = dto.Provider,
            AssociatedDomainId = domain.Id
        };

        if (userDb.IsSystemAccount)
        {
            sendMessageEntity.AssociatedDomainId = dto.AssociatedDomainId;
        }
        // Try saving the entity
        Result<SendMessage> saveResult = await sendMessageRepository.InsertAsync(sendMessageEntity, cancellationToken);

        // If saving fails, return the view with an error message
        if (!saveResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, ConstMessages.ErrorInSaving); // Add a general error message
            return View(dto); // Return the view with errors
        }

        // Log the creation and save modification details
        Modification modification = new()
        {
            Id = Guid.NewGuid().ToString(),
            ActionTypes = ActionTypes.Insert,
            CollectionType = CollectionType.SendMessage,
            Ip = controllerHelper.GetUserIpAddress(),
            ModifierId = userDb.Id,
            ModifierUserName = userDb.UserName,
            ModifyDateTime = DateTime.Now,
            RecordId = sendMessageEntity.Id
        };

        await modificationRepository.InsertAsync(modification, cancellationToken);

        logger.Information($"User {userDb.Id} ({userDb.UserName}) created SendMessage with ID {sendMessageEntity.Id}.");

        // Redirect to the List action after successful operation
        return RedirectToAction("List", new { Status = "Success", Message = ConstMessages.SuccessfullyDone });
    }

    [HttpPost]
    public async Task<IActionResult> EditSms(SmsDto dto, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        if (!ModelState.IsValid)
        {
            List<AjaxValidationErrorModel> errors = ModelState.Values
                                                              .SelectMany(v => v.Errors)
                                                              .Select(e => new AjaxValidationErrorModel { Key = e.ErrorMessage, ErrorMessage = e.ErrorMessage })
                                                              .ToList();

            return Json(new { Status = "ModelError", ModelStateErrors = errors });
        }

        SendMessage sendMessage = await sendMessageRepository.FirstOrDefaultAsync(c => c.SendType == SendType.SMS, cancellationToken);
        if (sendMessage == null)
        {
            return NotFound();
        }

        sendMessage = mapper.Map(dto, sendMessage);
        sendMessage.SendMessageMetaData = mapper.Map<Sms>(dto);
        Result<SendMessage> updateResult = await sendMessageRepository.UpdateAsync(sendMessage, cancellationToken);

        if (updateResult.Succeeded)
        {
            Modification modification = new()
                                        {
                Id = Guid.NewGuid().ToString(),
                ActionTypes = ActionTypes.Update,
                CollectionType = CollectionType.SendMessage,
                Ip = controllerHelper.GetUserIpAddress(),
                ModifierId = userDb.Id,
                ModifierUserName = userDb.UserName,
                ModifyDateTime = DateTime.Now,
                RecordId = sendMessage.Id
            };

            await modificationRepository.InsertAsync(modification, cancellationToken);

            logger.Information($"User {userDb.Id} ({userDb.UserName}) edited SendMessage with ID {sendMessage.Id}.");
            return Json(new { Status = "Success", Message = ConstMessages.SuccessfullyDone });
        }
        else
        {
            return Json(new { Status = "Error", Message = ConstMessages.ErrorInSaving });
        }
    }

    [HttpGet]
    public async Task<IActionResult> AddEditSmtp(string id, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        if (userDb.IsSystemAccount)
        {
            ViewBag.Domains = controllerHelper.GetAllActiveDomains();
        }

        ViewBag.IsSysAcc = userDb.IsSystemAccount;

        if (string.IsNullOrEmpty(id))
        {
            return View(new SmtpDto());
        }

        SendMessage sendMessage = await sendMessageRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (sendMessage is not { SendType: SendType.Email })
        {
            return NotFound();
        }

        Smtp smtp = JsonConvert.DeserializeObject<Smtp>(sendMessage.SendMessageMetaData.ToString());
        SmtpDto smtpDto = mapper.Map<SmtpDto>(smtp);
        smtpDto.Id = sendMessage.Id;
        smtpDto.AssociatedDomainId = sendMessage.AssociatedDomainId;

        return View(smtpDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSmtp(SmtpDto dto, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);
        Domain domain = controllerHelper.GetCurrentUserDomain();
        // If the model is invalid, return the view with validation errors.
        if (!ModelState.IsValid)
        {
            // Populate necessary ViewBag properties (if any are needed)
            ViewBag.Domains = controllerHelper.GetAllActiveDomains();
            ViewBag.IsSysAcc = userDb.IsSystemAccount;
            return View("AddEditSmtp", dto); // Return the view with the current dto and ModelState errors
        }

        // Map SmsDto to SendMessage entity
        SendMessage sendMessageEntity = new()
        {
            SendType = SendType.Email,
            SendMessageMetaData = mapper.Map<Smtp>(dto),
            CreationDate = DateTime.Now,
            IsDeleted = false,
            IsActive = true,
            CreatorUserId = userDb.Id,
            CreatorUserName = userDb.UserName,
            Id = Guid.NewGuid().ToString(),
            AssociatedDomainId = domain.Id
        };

        if (userDb.IsSystemAccount)
        {
            sendMessageEntity.AssociatedDomainId = dto.AssociatedDomainId;
        }
        // Try saving the entity
        Result<SendMessage> saveResult = await sendMessageRepository.InsertAsync(sendMessageEntity, cancellationToken);

        // If saving fails, return the view with an error message
        if (!saveResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, ConstMessages.ErrorInSaving); // Add a general error message
            return View("AddEditSmtp", dto); // Return the view with errors
        }

        // Log the creation and save modification details
        Modification modification = new()
        {
            Id = Guid.NewGuid().ToString(),
            ActionTypes = ActionTypes.Insert,
            CollectionType = CollectionType.SendMessage,
            Ip = controllerHelper.GetUserIpAddress(),
            ModifierId = userDb.Id,
            ModifierUserName = userDb.UserName,
            ModifyDateTime = DateTime.Now,
            RecordId = sendMessageEntity.Id
        };

        await modificationRepository.InsertAsync(modification, cancellationToken);

        logger.Information($"User {userDb.Id} ({userDb.UserName}) created SendMessage with ID {sendMessageEntity.Id}.");

        // Redirect to the List action after successful operation
        return RedirectToAction("List", new { Status = "Success", Message = ConstMessages.SuccessfullyDone });
    }

    [HttpPost]
    public async Task<IActionResult> EditSmtp(SmtpDto dto, CancellationToken cancellationToken)
    {
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        if (!ModelState.IsValid)
        {
            List<AjaxValidationErrorModel> errors = ModelState.Values
                                                              .SelectMany(v => v.Errors)
                                                              .Select(e => new AjaxValidationErrorModel { Key = e.ErrorMessage, ErrorMessage = e.ErrorMessage })
                                                              .ToList();

            return Json(new { Status = "ModelError", ModelStateErrors = errors });
        }

        SendMessage sendMessage = await sendMessageRepository.FirstOrDefaultAsync(c => c.SendType == SendType.Email, cancellationToken);
        if (sendMessage == null)
        {
            return NotFound();
        }

        sendMessage = mapper.Map(dto, sendMessage);
        Result<SendMessage> updateResult = await sendMessageRepository.UpdateAsync(sendMessage, cancellationToken);

        if (updateResult.Succeeded)
        {
            Modification modification = new Modification
            {
                Id = Guid.NewGuid().ToString(),
                ActionTypes = ActionTypes.Update,
                CollectionType = CollectionType.SendMessage,
                Ip = controllerHelper.GetUserIpAddress(),
                ModifierId = userDb.Id,
                ModifierUserName = userDb.UserName,
                ModifyDateTime = DateTime.Now,
                RecordId = sendMessage.Id
            };

            await modificationRepository.InsertAsync(modification, cancellationToken);

            logger.Information($"User {userDb.Id} ({userDb.UserName}) edited SendMessage with ID {sendMessage.Id}.");
            return Json(new { Status = "Success", Message = ConstMessages.SuccessfullyDone });
        }
        else
        {
            return Json(new { Status = "Error", Message = ConstMessages.ErrorInSaving });
        }
    }

    [HttpPost]
    public async ValueTask<IActionResult> Restore(string id, CancellationToken cancellationToken)
    {
        JsonResult result;
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            SendMessage sendMessageEntity = await sendMessageRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (sendMessageEntity == null)
            {
                result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_EntityNotFound") });
            }
            else
            {
                Result<SendMessage> res = await sendMessageRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, false, cancellationToken);

                if (res.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Restore,
                        CollectionType = CollectionType.SendMessage,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = sendMessageEntity.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    res.Message = ConstMessages.SuccessfullyDone;
                    result = new(new { Status = "success", Message = UtilityLanguage.GetString("AlertAndMessage_EditionDoneSuccessfully") });
                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, restoring content category with {id} id done successfully.");
                }
                else
                {
                    res.Message = ConstMessages.ErrorInSaving;
                    result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
                }
            }
        }
        catch (Exception e)
        {
            result = new(new { Status = "error", Message = UtilityLanguage.GetString("AlertAndMessage_TryLater") });
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SendMessageController)}/{nameof(Restore)}");
        }

        return result;
    }

    [HttpPost]
    public async ValueTask<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        Result<SendMessage> opResult = new();
        ApplicationUser userDb = await controllerHelper.GetCurrentUser(cancellationToken);

        try
        {
            SendMessage sendMessageEntity = await sendMessageRepository.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (sendMessageEntity != null)
            {
                opResult = await sendMessageRepository.UpdateAsync(c => c.Id == id, m => m.IsDeleted, true, cancellationToken);

                if (opResult.Succeeded)
                {
                    Modification modification = new()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActionTypes = ActionTypes.Delete,
                        CollectionType = CollectionType.SendMessage,
                        Ip = controllerHelper.GetUserIpAddress(),
                        ModifierId = userDb.Id,
                        ModifierUserName = userDb.UserName,
                        ModifyDateTime = DateTime.Now,
                        RecordId = sendMessageEntity.Id
                    };
                    Result<Modification> modifyInsert = await modificationRepository.InsertAsync(modification, cancellationToken);

                    logger.Information($"userId: {userDb.Id}, userName: {userDb.UserName}, deleting content category with {sendMessageEntity.Id} id done successfully.");
                    opResult.Message = ConstMessages.SuccessfullyDone;
                }
                else
                {
                    opResult.Message = ConstMessages.GeneralError;
                }
            }
            else
            {
                opResult.Message = UtilityLanguage.GetString("AlertAndMessage_ObjectNotFound");
            }
        }
        catch (Exception e)
        {
            logger.Error($"userId: {userDb.Id}, userName: {userDb.UserName} error {e.Message} occured. stack trace: {nameof(SendMessageController)}/{nameof(Delete)}");
        }

        return Json(opResult.Succeeded
                        ? new { Status = "Success", opResult.Message }
                        : new { Status = "Error", opResult.Message });
    }
}