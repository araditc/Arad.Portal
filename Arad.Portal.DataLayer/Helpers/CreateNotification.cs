// 
//  --------------------------------------------------------------------
//  Copyright (c) 2005-2021 Arad ITC.
// 
//  Author : Akbar Aslani <aslani@arad-itc.org>
//  Licensed under the Apache License, Version 2.0 (the "License")
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  --------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.MessageTemplate;
using Arad.Portal.DataLayer.Entities.General.Notify;
using Arad.Portal.DataLayer.Entities.General.SendMessage;
using Arad.Portal.DataLayer.Entities.General.SMS;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.MessageTemplate;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Notification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SendMessage;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.User;
using Arad.Portal.GeneralLibrary.Utilities;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Helpers;

public class CreateNotification(
    IMessageTemplateRepository messageTemplateRepository,
    INotificationRepository notificationRepository,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    Setting setting,
    ILanguageRepository languageRepository,
    IUserRepository userRepository,
    ISendMessageRepository sendMessageRepository,
    Sms sendSmsConfig,
    IDomainRepository domainRepository)
{
    public async Task<Result> ClientSignUp(string templateName, ApplicationUser user, string password, CancellationToken cancellationToken)
    {
        string domainName = $"{httpContextAccessor.HttpContext.Request.Host}";
        Domain domainEntity = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == "https://" + domainName, cancellationToken);
        SendMessage smtp = await sendMessageRepository.FirstOrDefaultAsync(c => c.AssociatedDomainId == domainEntity.Id, cancellationToken);
        Language language = await languageRepository.FirstOrDefaultAsync(c => c.Symbol == CultureInfo.CurrentCulture.Name, cancellationToken);
        if (smtp == null)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmtp") };
        }

        if (sendSmsConfig == null)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmsSetting") };
        }

        List<MessageTemplate> messageTemplates = await messageTemplateRepository.GetListAsync(c => c.TemplateName == templateName, cancellationToken);

        if (!messageTemplates.Any())
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundMessageTemplate") };
        }

        List<NotificationType> notificationTypes = [NotificationType.Sms];
        List<Notification> notifications = [];

        foreach (NotificationType notificationType in notificationTypes)
        {
            MessageTemplate messageTemplate = messageTemplates
                .FirstOrDefault(m => m.NotificationType == notificationType &&
                                     m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

            if (messageTemplate == null)
            {
                continue;
            }

            string title = GenerateText(notificationType,
                                        messageTemplate.MessageTemplateMultiLingual
                                                       .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name))
                                                       .Subject,
                                        user);
            string messageText = GenerateText(notificationType,
                                              messageTemplate.MessageTemplateMultiLingual
                                                             .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name))
                                                             .Body,
                                              user,
                                              "",
                                              "",
                                              password,
                                              "",
                                              domainEntity.Titles.FirstOrDefault(c => c.LanguageId == language.Id)?.Name ?? "");

            Notification notification = new()
            {
                Type = notificationType,
                ActionType = ActionType.NoExtraAction,
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                ScheduleDate = DateTime.Now.ToUniversalTime(),
                SendStatus = NotificationSendStatus.Store,
                Title = title,
                Body = messageText,
                CreationDate = DateTime.Now.ToUniversalTime(),
                CreatorUserId = user.Id,
                CreatorUserName = user.UserName,
                TemplateName = templateName,
                UserFullName = "",
                SendMessageMetaData = sendSmsConfig,
                AssociatedDomainId = (await domainRepository.FirstOrDefaultAsync(c => c.DomainName == "https://" + domainName, cancellationToken)).Id
            };

            switch (notificationType)
            {
                case NotificationType.Email:
                    notification.SendMessageMetaData = smtp;
                    notification.UserEmail = user.Email;

                    break;

                case NotificationType.Sms:
                    notification.SendMessageMetaData = sendSmsConfig;
                    notification.UserPhoneNumber = user.PhoneNumber;

                    break;
            }

            notifications.Add(notification);
        }

        if (notifications.Any())
        {
            await notificationRepository.InsertAsync(notifications, cancellationToken);
        }

        return new() { Succeeded = true, Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") };
    }

    public async Task<Result> NotifySiteAdminForLackOfInventory(string templateName, string productName, ApplicationUser adminUser, NotificationType notificationType, CancellationToken cancellationToken)
    {
        try
        {

            SendMessage smtp = new();
            string domainName = $"{httpContextAccessor.HttpContext.Request.Host}";
            Domain domainEntity = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == domainName && c.IsDefault == false, cancellationToken);
            Language language = await languageRepository.FirstOrDefaultAsync(c => c.Symbol == CultureInfo.CurrentCulture.Name, cancellationToken);
            switch (notificationType)
            {
                case NotificationType.Sms when sendSmsConfig == null:
                    return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmsSetting") };

                case NotificationType.Email:
                    {
                        smtp = await sendMessageRepository.FirstOrDefaultAsync(c => c.AssociatedDomainId == domainEntity.Id, cancellationToken);

                        if (smtp == null)
                        {
                            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmtp") };
                        }

                        break;
                    }
            }

            List<MessageTemplate> messageTemplates = await messageTemplateRepository.GetListAsync(c => c.TemplateName == templateName, cancellationToken);

            if (!messageTemplates.Any())
            {
                return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundMessageTemplate") };
            }

            MessageTemplate messageTemplate = messageTemplates
                .FirstOrDefault(m => m.NotificationType == NotificationType.Sms &&
                                     m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

            if (messageTemplate == null)
            {
                return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundMessageTemplate") };
            }

            string title = GenerateText(notificationType,
                                        messageTemplate.MessageTemplateMultiLingual
                                                       .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name))
                                                       .Subject,
                                        null,
                                        "",
                                        "",
                                        "",
                                        productName);
            string messageText = GenerateText(NotificationType.Sms,
                                              messageTemplate.MessageTemplateMultiLingual
                                                             .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name))
                                                             .Body,
                                              null,
                                              "",
                                              "",
                                              "",
                                              productName,
                                              domainEntity.Titles.FirstOrDefault(c => c.LanguageId == language.Id)?.Name ?? "");

            Notification notification = new()
            {
                Type = notificationType,
                ActionType = ActionType.NoExtraAction,
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                ScheduleDate = DateTime.Now.ToUniversalTime(),
                SendStatus = NotificationSendStatus.Store,
                Title = title,
                Body = messageText,
                CreationDate = DateTime.Now.ToUniversalTime(),
                CreatorUserId = adminUser.Id,
                CreatorUserName = adminUser.UserName,
                TemplateName = templateName,
                UserFullName = adminUser.Profile.FullName,
                SendMessageMetaData = smtp,
                AssociatedDomainId = (await domainRepository.FirstOrDefaultAsync(c => c.DomainName == domainName && c.IsDefault == true, cancellationToken)).Id,
                UserPhoneNumber = adminUser.PhoneNumber,
                ExtraData = []
            };

            await notificationRepository.InsertAsync([notification], cancellationToken);

            return new() { Succeeded = true, Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") };
        }
        catch (Exception)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_InsertError") };
        }
    }

    public async Task<Result> GenerateProductNotificationToUsers(string templateName, Notification notification)
    {
        string domainName = $"{httpContextAccessor.HttpContext.Request.Host}";
        Domain domainEntity = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == domainName && c.IsDefault == false);
        Language language = await languageRepository.FirstOrDefaultAsync(c => c.Symbol == CultureInfo.CurrentCulture.Name);
        switch (notification.Type)
        {
            case NotificationType.Sms when sendSmsConfig == null:
                return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmsSetting") };

            case NotificationType.Email:
                {
                    SendMessage smtp = await sendMessageRepository.FirstOrDefaultAsync(c => c.AssociatedDomainId == domainEntity.Id);

                    if (smtp == null)
                    {
                        return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmtp") };
                    }

                    break;
                }

            case NotificationType.Notification:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        try
        {
            List<MessageTemplate> messageTemplates = await messageTemplateRepository.GetListAsync(c => c.TemplateName == templateName);
            MessageTemplate messageTemplate = messageTemplates
                .FirstOrDefault(m => m.NotificationType == notification.Type &&
                                     m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));
            List<(string, string)> extraData = notification.ExtraData;
            string productId = extraData.FirstOrDefault(t => t.Item1 == "productId").Item2;
            string productName = extraData.FirstOrDefault(t => t.Item1 == "productName").Item2;
            List<Notification> lst = [];
            IQueryable<string> selectedUserId = userManager.Users.Where(u => u.Profile.InformProductList.Contains(productId)).Select(u => u.Id);

            Parallel.ForEach(selectedUserId,
                             Action);

            await notificationRepository.InsertAsync(lst);
            notification.SendStatus = NotificationSendStatus.Posted;
            await notificationRepository.UpdateAsync(notification);

            return new() { Succeeded = true, Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") };

            async void Action(string userId)
            {
                ApplicationUser userEntity = await userManager.FindByIdAsync(userId);

                if (messageTemplate == null)
                {
                    return;
                }

                string title = GenerateText(notification.Type,
                                            messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name))
                                                           .Subject,
                                            userEntity,
                                            "",
                                            "",
                                            "",
                                            productName);
                string messageText = GenerateText(NotificationType.Sms,
                                                  messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name))
                                                                 .Body,
                                                  userEntity,
                                                  "",
                                                  "",
                                                  "",
                                                  productName,
                                                  domainEntity.Titles.FirstOrDefault(c => c.LanguageId == language.Id)?.Name ?? "");

                if (userEntity != null)
                {
                    lst.Add(new()
                    {
                        Type = notification.Type,
                        Id = Guid.NewGuid().ToString(),
                        ActionType = ActionType.ProductAvailibilityReminder,
                        IsActive = true,
                        ScheduleDate = DateTime.Now.ToUniversalTime(),
                        SendStatus = NotificationSendStatus.Store,
                        Title = title,
                        Body = messageText,
                        CreationDate = DateTime.Now.ToUniversalTime(),
                        CreatorUserId = userEntity.Id,
                        CreatorUserName = userEntity.UserName,
                        TemplateName = templateName,
                        UserFullName = userEntity.Profile.FullName,
                        AssociatedDomainId = (await domainRepository.FirstOrDefaultAsync(c => c.DomainName == domainName && c.IsDefault == true)).Id,
                        UserPhoneNumber = userEntity.PhoneNumber
                    });
                }
            }
        }
        catch (Exception)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_InsertError") };
        }
    }

    /// <summary>
    ///     whether it is used for user or site admin
    /// </summary>
    /// <param name="templateName"></param>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result> NotifyNewOrder(string templateName, ApplicationUser user, CancellationToken cancellationToken)
    {
        try
        {
            string domainName = $"{httpContextAccessor.HttpContext.Request.Host}";
            Domain domainEntity = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == domainName && c.IsDefault == false, cancellationToken);
            Language language = await languageRepository.FirstOrDefaultAsync(c => c.Symbol == CultureInfo.CurrentCulture.Name, cancellationToken);
            if (sendSmsConfig == null)
            {
                return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmsSetting") };
            }

            List<MessageTemplate> messageTemplates = await messageTemplateRepository.GetListAsync(c => c.TemplateName == templateName, cancellationToken);

            if (!messageTemplates.Any())
            {
                return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundMessageTemplate") };
            }

            MessageTemplate messageTemplate = messageTemplates
                .FirstOrDefault(m => m.NotificationType == NotificationType.Sms &&
                                     m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

            if (messageTemplate != null)
            {
                string title = GenerateText(NotificationType.Sms,
                                            messageTemplate.MessageTemplateMultiLingual
                                                           .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name))
                                                           .Subject,
                                            user);
                string messageText = GenerateText(NotificationType.Sms,
                                                  messageTemplate.MessageTemplateMultiLingual
                                                                 .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name))
                                                                 .Body,
                                                  user,
                                                  "",
                                                  "",
                                                  "",
                                                  "",
                                                  domainEntity.Titles.FirstOrDefault(c => c.LanguageId == language.Id)?.Name ?? "");

                Notification notification = new()
                {
                    Type = NotificationType.Sms,
                    Id = Guid.NewGuid().ToString(),
                    IsActive = true,
                    ScheduleDate = DateTime.Now.ToUniversalTime(),
                    SendStatus = NotificationSendStatus.Store,
                    Title = title,
                    Body = messageText,
                    CreationDate = DateTime.Now.ToUniversalTime(),
                    CreatorUserId = user.Id,
                    ActionType = ActionType.NoExtraAction,
                    CreatorUserName = user.UserName,
                    TemplateName = templateName,
                    UserFullName = user.Profile.FullName,
                    SendMessageMetaData = sendSmsConfig,
                    SendType = SendType.SMS,
                    AssociatedDomainId = (await domainRepository.FirstOrDefaultAsync(c => c.DomainName == domainName && c.IsDefault == true, cancellationToken)).Id,
                    UserPhoneNumber = user.PhoneNumber
                };

                await notificationRepository.InsertAsync([notification], cancellationToken);

                return new() { Succeeded = true, Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") };
            }

            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundMessageTemplate") };
        }
        catch (Exception)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_InsertError") };
        }
    }

    public async Task<Result> SendCustomMessage(ApplicationUser user, string messageText, CancellationToken cancellationToken)
    {
        string domainName = $"{httpContextAccessor.HttpContext.Request.Host}";
        Domain domainEntity = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == domainName, cancellationToken);
        SendMessage smtp = await sendMessageRepository.FirstOrDefaultAsync(c => c.AssociatedDomainId == domainEntity.Id, cancellationToken);

        if (sendSmsConfig == null)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmsSetting") };
        }

        List<NotificationType> notificationTypes = [NotificationType.Sms];
        List<Notification> notifications = [];

        foreach (NotificationType notificationType in notificationTypes)
        {
            Notification notification = new()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Type = notificationType,
                ScheduleDate = DateTime.Now.ToUniversalTime(),
                SendStatus = NotificationSendStatus.Store,
                Title = "",
                ActionType = ActionType.NoExtraAction,
                Body = messageText,
                CreationDate = DateTime.Now.ToUniversalTime(),
                CreatorUserId = user.Id,
                CreatorUserName = user.UserName,
                TemplateName = "",
                UserFullName = "",
                SendMessageMetaData = sendSmsConfig,
                AssociatedDomainId = (await domainRepository.FirstOrDefaultAsync(c => c.DomainName == domainName && c.IsDefault == true, cancellationToken)).Id
            };

            switch (notificationType)
            {
                case NotificationType.Email:
                    notification.SendMessageMetaData = smtp;
                    notification.UserEmail = user.Email;

                    break;

                case NotificationType.Sms:
                    notification.SendMessageMetaData = sendSmsConfig;
                    notification.UserPhoneNumber = user.PhoneNumber;

                    break;

                case NotificationType.Notification:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            notifications.Add(notification);
        }

        if (notifications.Any())
        {
            await notificationRepository.InsertAsync(notifications, cancellationToken);
        }

        return new() { Succeeded = true, Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") };
    }

    public async Task<Result> Send(string templateName, ApplicationUser user, string password, CancellationToken cancellationToken)
    {
        Domain domainEntity = await domainRepository.FirstOrDefaultAsync(c => c.IsDefault, cancellationToken);
        SendMessage smtp = await sendMessageRepository.FirstOrDefaultAsync(c => c.SendType == SendType.Email, cancellationToken);
        Language language = await languageRepository.FirstOrDefaultAsync(c => c.Symbol == CultureInfo.CurrentCulture.Name, cancellationToken);
        if (smtp == null)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmtp") };
        }

        if (sendSmsConfig == null)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmsSetting") };
        }

        List<MessageTemplate> messageTemplates = await messageTemplateRepository.GetListAsync(c => c.TemplateName == templateName, cancellationToken);

        if (!messageTemplates.Any())
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundMessageTemplate") };
        }

        List<NotificationType> notificationTypes = [NotificationType.Sms, NotificationType.Email];
        List<Notification> notifications = [];

        foreach (NotificationType notificationType in notificationTypes)
        {
            MessageTemplate messageTemplate = messageTemplates
                .FirstOrDefault(m => m.NotificationType == notificationType &&
                                     m.MessageTemplateMultiLingual
                                      .Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

            if (messageTemplate == null)
            {
                continue;
            }

            string title = GenerateText(notificationType,
                                        messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject,
                                        user);
            string messageText = GenerateText(notificationType,
                                              messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body,
                                              user,
                                              "",
                                              "",
                                              password,
                                              "",
                                              domainEntity.Titles.FirstOrDefault(c => c.LanguageId == language.Id)?.Name ?? "");

            Notification notification = new()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Type = notificationType,
                ActionType = ActionType.NoExtraAction,
                ScheduleDate = DateTime.Now.ToUniversalTime(),
                SendStatus = NotificationSendStatus.Store,
                Title = title,
                Body = messageText,
                CreationDate = DateTime.Now.ToUniversalTime(),
                CreatorUserId = user.Id,
                CreatorUserName = user.UserName,
                TemplateName = templateName,
                UserFullName = "",
                SendMessageMetaData = sendSmsConfig,
                AssociatedDomainId = (await domainRepository.FirstOrDefaultAsync(c => c.DomainName == domainEntity.DomainName && c.IsDefault == true, cancellationToken)).Id
            };

            switch (notificationType)
            {
                case NotificationType.Email:
                    notification.SendMessageMetaData = smtp;
                    notification.UserEmail = user.Email;

                    break;

                case NotificationType.Sms:
                    notification.SendMessageMetaData = sendSmsConfig;
                    notification.UserPhoneNumber = user.PhoneNumber;

                    break;

                case NotificationType.Notification:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            notifications.Add(notification);
        }

        if (notifications.Any())
        {
            await notificationRepository.InsertAsync(notifications, cancellationToken);
        }

        return new() { Succeeded = true, Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") };
    }

    public async Task<Result> SendConfirmEmail(string templateName, ApplicationUser user, string callbackUrl, CancellationToken cancellationToken)
    {
        string domainName = $"{httpContextAccessor.HttpContext.Request.Host}";
        Domain domainEntity = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == domainName, cancellationToken);
        SendMessage smtp = await sendMessageRepository.FirstOrDefaultAsync(c => c.AssociatedDomainId == domainEntity.Id, cancellationToken);

        if (smtp == null)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmtp") };
        }

        if (sendSmsConfig == null)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmsSetting") };
        }

        List<MessageTemplate> messageTemplates = await messageTemplateRepository.GetListAsync(c => c.TemplateName == templateName, cancellationToken);

        if (!messageTemplates.Any())
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundMessageTemplate") };
        }

        List<NotificationType> notificationTypes = [NotificationType.Email];
        List<Notification> notifications = [];

        foreach (NotificationType notificationType in notificationTypes)
        {
            MessageTemplate messageTemplate = messageTemplates.FirstOrDefault(m => m.NotificationType == notificationType && m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

            if (messageTemplate == null)
            {
                continue;
            }

            string title = GenerateText(notificationType,
                                        messageTemplate.MessageTemplateMultiLingual
                                                       .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name))
                                                       .Subject,
                                        user);
            string messageText = GenerateText(notificationType,
                                              messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body,
                                              user,
                                              callbackUrl);

            Notification notification = new()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Type = notificationType,
                ActionType = ActionType.NoExtraAction,
                ScheduleDate = DateTime.Now.ToUniversalTime(),
                SendStatus = NotificationSendStatus.Store,
                Title = title,
                Body = messageText,
                CreationDate = DateTime.Now.ToUniversalTime(),
                CreatorUserId = user.Id,
                CreatorUserName = user.UserName,
                TemplateName = templateName,
                UserFullName = user.Profile.FullName,
                AssociatedDomainId = (await domainRepository.FirstOrDefaultAsync(c => c.DomainName == domainName && c.IsDefault == true, cancellationToken)).Id
            };

            switch (notificationType)
            {
                case NotificationType.Email:
                    notification.SendMessageMetaData = smtp;
                    notification.UserEmail = user.Email;

                    break;

                case NotificationType.Sms:
                    notification.SendMessageMetaData = sendSmsConfig;
                    notification.UserPhoneNumber = user.PhoneNumber;

                    break;

                case NotificationType.Notification:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            notifications.Add(notification);
        }

        if (notifications.Any())
        {
            await notificationRepository.InsertAsync(notifications, cancellationToken);
        }

        return new() { Succeeded = true, Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") };
    }

    public async Task<Result> SendOtp(string templateName, ApplicationUser user, string otp, CancellationToken cancellationToken)
    {
        string domainName = $"{httpContextAccessor.HttpContext.Request.Host}";
        Domain domainEntity = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == domainName && c.IsDefault == false, cancellationToken);
        SendMessage smtp = await sendMessageRepository.FirstOrDefaultAsync(c => c.AssociatedDomainId == domainEntity.Id, cancellationToken);
        Language language = await languageRepository.FirstOrDefaultAsync(c => c.Symbol == CultureInfo.CurrentCulture.Name, cancellationToken);
        if (smtp == null)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmtp") };
        }

        if (sendSmsConfig == null)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmsSetting") };
        }

        List<MessageTemplate> messageTemplates = await messageTemplateRepository.GetListAsync(c => c.TemplateName == templateName, cancellationToken);

        if (!messageTemplates.Any())
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundMessageTemplate") };
        }

        List<NotificationType> notificationTypes = [NotificationType.Sms];
        List<Notification> notifications = [];

        foreach (NotificationType notificationType in notificationTypes)
        {
            MessageTemplate messageTemplate = messageTemplates.FirstOrDefault(m => m.NotificationType == notificationType && m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

            if (messageTemplate == null)
            {
                continue;
            }

            string title = GenerateText(notificationType,
                                        messageTemplate.MessageTemplateMultiLingual
                                                       .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name))
                                                       .Subject,
                                        user);
            string messageText = GenerateText(
                notificationType,
                messageTemplate.MessageTemplateMultiLingual
                               .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name))
                               .Body,
                user,
                "",
                otp,
                "",
                domainEntity.Titles.FirstOrDefault(c => c.LanguageId == language.Id)?.Name ?? "");

            Notification notification = new()
            {
                Id = Guid.NewGuid().ToString(),
                IsActive = true,
                Type = notificationType,
                ActionType = ActionType.NoExtraAction,
                ScheduleDate = DateTime.UtcNow,
                SendStatus = NotificationSendStatus.Store,
                Title = title,
                Body = messageText,
                CreationDate = DateTime.UtcNow,
                CreatorUserId = user.Id,
                CreatorUserName = user.UserName,
                TemplateName = templateName,
                UserFullName = user.Profile.FullName,
                SendMessageMetaData = sendSmsConfig,
                SendType = SendType.SMS,
                AssociatedDomainId = (await domainRepository
                                                                      .FirstOrDefaultAsync(c => c.DomainName == domainName && c.IsDefault == true, cancellationToken))?.Id
            };

            switch (notificationType)
            {
                case NotificationType.Email:
                    notification.SendMessageMetaData = smtp;
                    notification.UserEmail = user.Email;

                    break;

                case NotificationType.Sms:
                    notification.SendMessageMetaData = sendSmsConfig;
                    notification.UserPhoneNumber = user.PhoneNumber;

                    break;

                case NotificationType.Notification:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            notifications.Add(notification);
        }

        if (notifications.Any())
        {
            await notificationRepository.InsertAsync(notifications, cancellationToken);
        }

        return new() { Succeeded = true, Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") };
    }

    public async Task<Result> SendOtp(string templateName, string phoneNumber, string otp, CancellationToken cancellationToken)
    {
        try
        {
            ApplicationUser user = await userRepository.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber, cancellationToken);
            string domainName = $"{httpContextAccessor.HttpContext.Request.Host}";
            Domain domainEntity = await domainRepository.FirstOrDefaultAsync(c => c.DomainName == "https://" + domainName, cancellationToken);
            SendMessage smtp = await sendMessageRepository.FirstOrDefaultAsync(c => c.SendType == SendType.Email, cancellationToken);
            Language language = await languageRepository.FirstOrDefaultAsync(c => c.Symbol == CultureInfo.CurrentCulture.Name, cancellationToken);
            //if (smtp == null)
            //{
            //    return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmtp") };
            //}
            if (smtp == null && sendSmsConfig == null)
            {
                return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmtp") };
            }
            //if (sendSmsConfig == null)
            //{
            //    return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundSmsSetting") };
            //}

            List<MessageTemplate> messageTemplates = await messageTemplateRepository.GetListAsync(c => c.TemplateName == templateName, cancellationToken);

            if (!messageTemplates.Any())
            {
                return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_NotFoundMessageTemplate") };
            }

            List<NotificationType> notificationTypes;
            if (user == null)
            {
                notificationTypes = [NotificationType.Sms];
            }
            else
            {
                notificationTypes = [NotificationType.Sms, NotificationType.Email];
            }

            List<Notification> notifications = [];

            ApplicationUser systemUser = userManager.Users.FirstOrDefault(c => c.IsSystemAccount);

            if (systemUser == null)
            {
                return new() { Succeeded = false, Message = "System user not found" };
            }

            foreach (NotificationType notificationType in notificationTypes)
            {
                MessageTemplate messageTemplate = messageTemplates.FirstOrDefault(m => m.NotificationType == notificationType && m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

                if (messageTemplate == null)
                {
                    continue;
                }

                string title = GenerateText(notificationType, messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, null);
                string messageText = GenerateText(
                    notificationType,
                    messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body,
                    null,
                    null,
                    otp,
                    null,
                    domainEntity.Titles.FirstOrDefault(c => c.LanguageId == language.Id)?.Name ?? ""
                );

                Notification notification = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsActive = true,
                    Type = notificationType,
                    ActionType = ActionType.NoExtraAction,
                    ScheduleDate = DateTime.UtcNow,
                    SendStatus = NotificationSendStatus.Store,
                    Title = title,
                    Body = messageText,
                    CreationDate = DateTime.UtcNow,
                    CreatorUserId = systemUser.Id,
                    CreatorUserName = systemUser.UserName,
                    TemplateName = templateName,
                    UserFullName = "",
                    AssociatedDomainId = domainEntity?.Id
                };

                switch (notificationType)
                {

                    case NotificationType.Sms:
                        if (sendSmsConfig != null)
                        {
                            notification.SendMessageMetaData = sendSmsConfig;
                            notification.UserPhoneNumber = phoneNumber;
                        }

                        break;

                    case NotificationType.Email:
                        if (smtp != null)
                        {
                            notification.UserEmail = user?.Email;
                            notification.UserFullName = user?.Profile.FullName;
                            notification.SendMessageMetaData = smtp;
                        }

                        break;
                }

                notifications.Add(notification);
            }

            if (notifications.Any())
            {
                await notificationRepository.InsertAsync(notifications, cancellationToken);
            }

            return new() { Succeeded = true, Message = UtilityLanguage.GetString("AlertAndMessage_OperationSuccess") };
        }
        catch (Exception ex)
        {
            return new() { Succeeded = false, Message = UtilityLanguage.GetString("AlertAndMessage_Error") };
        }
    }

    private string GenerateText(NotificationType notificationType,
                                string text,
                                ApplicationUser user,
                                string callbackUrl = "",
                                string otp = "",
                                string passwordBeforeHash = "",
                                string productName = "",
                                string domainTitle = "")
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        if (notificationType == NotificationType.Sms)
        {
            text = text.ClearHtml();
            text = text.Replace(@"\n", Environment.NewLine);
        }

        text = text.Replace("{$date}", CultureInfo.CurrentCulture.Name == "fa-IR" ? DateTime.Now.ToPersianLetDateTime() : DateTime.Now.ToString(setting.DateFormat));
        text = text.Replace("{$time}", DateTime.Now.ToString("HH:mm"));
        text = text.Replace("{$otp}", IsNullOrEmpty(otp));
        text = text.Replace("{$domainName}", domainTitle);
        text = text.Replace("{$productName}", productName);
        text = text.Replace("{$pass}", passwordBeforeHash);

        if (!string.IsNullOrWhiteSpace(callbackUrl))
        {
            string url = $"<a href='{callbackUrl}'>{UtilityLanguage.GetString("Action_Confirmation")}</a>";
            text = text.Replace("{$client_email_verification_link}", url);
        }

        if (user != null)
        {
            text = text.Replace("{$client_id}", user.Id);
            text = text.Replace("{$client_name}", user.UserName);
            text = text.Replace("{$client_first_name}", IsNullOrEmpty(user.Profile.FirstName));
            text = text.Replace("{$client_last_name}", IsNullOrEmpty(user.Profile.LastName));

            text = text.Replace("{$client_email}", IsNullOrEmpty(user.Email));
            text = text.Replace("{$client_signup_date}", CultureInfo.CurrentCulture.Name == "fa-IR" ? user.CreationDate.ToPersianLetDateTime() : user.CreationDate.ToString(setting.DateFormat));
            text = text.Replace("{$client_phonenumber}", IsNullOrEmpty(user.PhoneNumber));
            text = text.Replace("{$client_status}", user.IsActive ? UtilityLanguage.GetString("Action_Active") : UtilityLanguage.GetString("Action_Inactive"));
            text = text.Replace("{$client_password}", passwordBeforeHash);
        }

        return text;

        string IsNullOrEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "" : value;
        }
    }
}