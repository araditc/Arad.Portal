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
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Contracts.General.MessageTemplate;
using Arad.Portal.DataLayer.Contracts.General.Notification;
using Arad.Portal.DataLayer.Contracts.General.SystemSetting;
using Arad.Portal.DataLayer.Contracts.General.User;
using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Entities.General.MessageTemplate;
using Arad.Portal.DataLayer.Entities.General.Notify;
using Arad.Portal.DataLayer.Entities.General.SystemSetting;
using Arad.Portal.DataLayer.Entities.General.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
using Arad.Portal.DataLayer.Contracts.General.Domain;
using Microsoft.AspNetCore.Hosting;
using Arad.Portal.DataLayer.Contracts.Shop.Product;
using Arad.Portal.DataLayer.Contracts.General.Language;

namespace Arad.Portal.DataLayer.Helpers
{
    public class CreateNotification
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Setting _setting;
        private readonly SendSmsConfig _sendSmsConfig;
        private readonly IMessageTemplateRepository _messageTemplateRepository;
        private readonly ISystemSettingRepository _systemSettingRepository;
        private readonly INotificationRepository _notificationRepository;
        //private readonly ISMTPRepository _smtpRepository;
        private readonly IDomainRepository _domainRepository;
        //private readonly IUserRepository _userManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILanguageRepository _lanRepository;
        private readonly IWebHostEnvironment _env;
        private  string _domainName;

        public CreateNotification(IMessageTemplateRepository messageTemplateRepository,
                                  ISystemSettingRepository systemSettingRepository,
                                  INotificationRepository notificationRepository,
                                  UserManager<ApplicationUser> userManager,
                                  IHttpContextAccessor httpContextAccessor,
                                  Setting setting,
                                  ILanguageRepository lanRepository,
                                  SendSmsConfig sendSmsConfig,
                                  IDomainRepository domainRepository,
                                  IWebHostEnvironment environment
                                  )
        {
            _messageTemplateRepository = messageTemplateRepository;
            _systemSettingRepository = systemSettingRepository;
            _notificationRepository = notificationRepository;
            //_smtpRepository = smtpRepository;
            _userManager = userManager;
            _env = environment;
            _httpContextAccessor = httpContextAccessor;
            _setting = setting;
            _sendSmsConfig = sendSmsConfig;
            _lanRepository = lanRepository;
            _domainRepository = domainRepository;
           // _domainName = $"{_httpContextAccessor.HttpContext??.Request.Host}";

        }
       
        public async Task<Result> ClientSignUp(string templateName, ApplicationUser user, string password)
        {
            _domainName = $"{_httpContextAccessor.HttpContext.Request.Host}";
            SMTP smtp = _domainRepository.GetSMTPAccount(_domainName);

            if (smtp == null)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmtp") };
            }

            if (_sendSmsConfig == null)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmsSetting") };
            }

            List<MessageTemplate> messageTemplates = await _messageTemplateRepository.GetAllByName(templateName);

            if (!messageTemplates.Any())
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundMessageTemplate") };
            }

            List<NotificationType> notificationTypes = new() { NotificationType.Sms };
            List<Notification> notifications = new();

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
                    .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, user);
                string messageText = GenerateText(notificationType, messageTemplate.MessageTemplateMultiLingual
                    .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body, user, "", "", password);

                Notification notification = new()
                {
                    Type = notificationType,
                    ActionType = ActionType.NoExtraAction,
                    NotificationId = Guid.NewGuid().ToString(),
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
                    SendSmsConfig = _sendSmsConfig,
                    AssociatedDomainId = _domainRepository.FetchByName(_domainName, true).ReturnValue.DomainId
                };

                switch (notificationType)
                {
                    case NotificationType.Email:
                        notification.SMTP = smtp;
                        notification.UserEmail = user.Email;

                        break;

                    case NotificationType.Sms:
                        notification.SendSmsConfig = _sendSmsConfig;
                        notification.UserPhoneNumber = user.PhoneNumber;

                        break;
                }

                notifications.Add(notification);
            }

            if (notifications.Any())
            {
                await _notificationRepository.AddRange(notifications);
            }
            return new() { Succeeded = true, Message = Language.GetString("AlertAndMessage_OperationSuccess") };
        }


        public async Task<Result> NotifySiteAdminForLackOfInventory(string templateName, string productName, ApplicationUser adminUser, NotificationType notificationType)
        {
            try
            {
                _domainName = $"{_httpContextAccessor.HttpContext.Request.Host}";
                SMTP smtpAccount = null;
                if (notificationType == NotificationType.Sms)
                {
                    if (_sendSmsConfig == null)
                    {
                        return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmsSetting") };
                    }
                }
                else if (notificationType == NotificationType.Email)
                {
                    smtpAccount = _domainRepository.GetSMTPAccount(_domainName);

                    if (smtpAccount == null)
                    {
                        return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmtp") };
                    }
                }
                List<MessageTemplate> messageTemplates = await _messageTemplateRepository.GetAllByName(templateName);

                if (!messageTemplates.Any())
                {
                    return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundMessageTemplate") };
                }
                MessageTemplate messageTemplate = messageTemplates
                  .FirstOrDefault(m => m.NotificationType == NotificationType.Sms &&
                  m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

                if (messageTemplate != null)
                {
                    string title = GenerateText(notificationType,
               messageTemplate.MessageTemplateMultiLingual
               .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, null, "", "", "", productName);
                    string messageText = GenerateText(NotificationType.Sms, messageTemplate.MessageTemplateMultiLingual
                        .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body, null, "", "", "", productName);

                    Notification notification = new()
                    {
                        Type = notificationType,
                        ActionType = ActionType.NoExtraAction,
                        NotificationId = Guid.NewGuid().ToString(),
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
                        SendSmsConfig = _sendSmsConfig,
                        SMTP = smtpAccount,
                        AssociatedDomainId = _domainRepository.FetchByName(_domainName, true).ReturnValue.DomainId,
                        UserPhoneNumber = adminUser.PhoneNumber,
                        ExtraData = new List<(string, string)>()
                    };

                    await _notificationRepository.AddRange(new List<Notification>() { notification });
                    return new() { Succeeded = true, Message = Language.GetString("AlertAndMessage_OperationSuccess") };

                }
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundMessageTemplate") };
            }
            catch (Exception)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_InsertError") };
            }
        }


        public async Task<Result> GenerateProductNotificationToUsers(string templateName, Notification notification)
        {
            SMTP smtpAccount = null;
            _domainName = $"{_httpContextAccessor.HttpContext.Request.Host}";
            if (notification.Type == NotificationType.Sms)
            {
                if (_sendSmsConfig == null)
                {
                    return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmsSetting") };
                }
            }
            else if (notification.Type == NotificationType.Email)
            {
                smtpAccount = _domainRepository.GetSMTPAccount(_domainName);

                if (smtpAccount == null)
                {
                    return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmtp") };
                }
            }
            try
            {
                List<MessageTemplate> messageTemplates = await _messageTemplateRepository.GetAllByName(templateName);
                MessageTemplate messageTemplate = messageTemplates
                   .FirstOrDefault(m => m.NotificationType == notification.Type &&
                   m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));
                var extraData = notification.ExtraData;
                var productId = extraData.FirstOrDefault(_ => _.Item1 == "productId").Item2;
                var productName = extraData.FirstOrDefault(_ => _.Item1 == "productName").Item2;
                List<Notification> lst = new List<Notification>();
                var selectedUserId = _userManager.Users.Where(_ => _.Profile.InformProductList.Contains(productId)).Select(_ => _.Id);
               

                Parallel.ForEach(selectedUserId, async userId =>
                {
                    var userEntity = await _userManager.FindByIdAsync(userId);

                    string title = GenerateText(notification.Type,
             messageTemplate.MessageTemplateMultiLingual
             .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, userEntity, "", "", "", productName);
                    string messageText = GenerateText(NotificationType.Sms, messageTemplate.MessageTemplateMultiLingual
                        .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body, userEntity, "", "", "", productName);

                    lst.Add(new Notification()
                    {
                        Type = notification.Type,
                        NotificationId = Guid.NewGuid().ToString(),
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
                        SendSmsConfig = _sendSmsConfig,
                        SMTP = smtpAccount,
                        AssociatedDomainId = _domainRepository.FetchByName(_domainName, true).ReturnValue.DomainId,
                        UserPhoneNumber = userEntity.PhoneNumber
                    });
                });

                await _notificationRepository.AddRange(lst);
                notification.SendStatus = NotificationSendStatus.Posted;
                await _notificationRepository.Update(notification, "");
                return new() { Succeeded = true, Message = Language.GetString("AlertAndMessage_OperationSuccess") };
            }
            catch (Exception)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_InsertError") };
            }

        }

        /// <summary>
        /// whethere it is used for user or site admin
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Result> NotifyNewOrder(string templateName, ApplicationUser user)
        {
            try
            {
                _domainName = $"{_httpContextAccessor.HttpContext.Request.Host}";
                if (_sendSmsConfig == null)
                {
                    return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmsSetting") };
                }
                List<MessageTemplate> messageTemplates = await _messageTemplateRepository.GetAllByName(templateName);

                if (!messageTemplates.Any())
                {
                    return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundMessageTemplate") };
                }
                MessageTemplate messageTemplate = messageTemplates
                        .FirstOrDefault(m => m.NotificationType == NotificationType.Sms &&
                        m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

                if (messageTemplate != null)
                {
                    string title = GenerateText(NotificationType.Sms,
               messageTemplate.MessageTemplateMultiLingual
               .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, user);
                    string messageText = GenerateText(NotificationType.Sms, messageTemplate.MessageTemplateMultiLingual
                        .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body, user, "", "", "");

                    Notification notification = new()
                    {
                        Type = NotificationType.Sms,
                        NotificationId = Guid.NewGuid().ToString(),
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
                        SendSmsConfig = _sendSmsConfig,
                        AssociatedDomainId = _domainRepository.FetchByName(_domainName, true).ReturnValue.DomainId,
                        UserPhoneNumber = user.PhoneNumber
                    };

                    await _notificationRepository.AddRange(new List<Notification>() { notification });
                    return new() { Succeeded = true, Message = Language.GetString("AlertAndMessage_OperationSuccess") };

                }
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundMessageTemplate") };
            }
            catch (Exception)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_InsertError") };
            }

        }
        public async Task<Result> SendCustomMessage(ApplicationUser user, string messageText)
        {
            _domainName = $"{_httpContextAccessor.HttpContext.Request.Host}";
            SMTP smtp = _domainRepository.GetSMTPAccount(_domainName);
            if (_sendSmsConfig == null)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmsSetting") };
            }

            List<NotificationType> notificationTypes = new() { NotificationType.Sms };
            List<Notification> notifications = new();

            foreach (NotificationType notificationType in notificationTypes)
            {
                Notification notification = new()
                {
                    NotificationId = Guid.NewGuid().ToString(),
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
                    SendSmsConfig = _sendSmsConfig,
                    AssociatedDomainId = _domainRepository.FetchByName(_domainName, true).ReturnValue.DomainId
                };

                switch (notificationType)
                {
                    case NotificationType.Email:
                        notification.SMTP = smtp;
                        notification.UserEmail = user.Email;

                        break;

                    case NotificationType.Sms:
                        notification.SendSmsConfig = _sendSmsConfig;
                        notification.UserPhoneNumber = user.PhoneNumber;

                        break;
                }

                notifications.Add(notification);
            }

            if (notifications.Any())
            {
                await _notificationRepository.AddRange(notifications);
            }

            return new() { Succeeded = true, Message = Language.GetString("AlertAndMessage_OperationSuccess") };
        }

        public async Task<Result> Send(string templateName, ApplicationUser user, string password)
        {
            //SMTP smtp = await _smtpRepository.GetDefault();
            _domainName = $"{_httpContextAccessor.HttpContext.Request.Host}";
            SMTP smtp = _domainRepository.GetSMTPAccount(_domainName);

            if (smtp == null)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmtp") };
            }

            if (_sendSmsConfig == null)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmsSetting") };
            }

            List<MessageTemplate> messageTemplates = await _messageTemplateRepository.GetAllByName(templateName);

            if (!messageTemplates.Any())
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundMessageTemplate") };
            }

            List<NotificationType> notificationTypes = new() { NotificationType.Sms, NotificationType.Email };
            List<Notification> notifications = new();

            foreach (NotificationType notificationType in notificationTypes)
            {
                MessageTemplate messageTemplate = messageTemplates
                    .FirstOrDefault(m => m.NotificationType == notificationType && m.MessageTemplateMultiLingual
                    .Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

                if (messageTemplate == null)
                {
                    continue;
                }

                string title = GenerateText(notificationType,
                    messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, user);
                string messageText = GenerateText(notificationType,
                    messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body, user, "", "", password);

                Notification notification = new()
                {
                    NotificationId = Guid.NewGuid().ToString(),
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
                    SendSmsConfig = _sendSmsConfig,
                    AssociatedDomainId = _domainRepository.FetchByName(_domainName, true).ReturnValue.DomainId
                };

                switch (notificationType)
                {
                    case NotificationType.Email:
                        notification.SMTP = smtp;
                        notification.UserEmail = user.Email;

                        break;

                    case NotificationType.Sms:
                        notification.SendSmsConfig = _sendSmsConfig;
                        notification.UserPhoneNumber = user.PhoneNumber;

                        break;
                }

                notifications.Add(notification);
            }

            if (notifications.Any())
            {
                await _notificationRepository.AddRange(notifications);
            }

            return new() { Succeeded = true, Message = Language.GetString("AlertAndMessage_OperationSuccess") };
        }

        public async Task<Result> SendConfirmEmail(string templateName, ApplicationUser user, string callbackUrl)
        {
            //SMTP smtp = await _smtpRepository.GetDefault(); 
            _domainName = $"{_httpContextAccessor.HttpContext.Request.Host}";
            SMTP smtp = _domainRepository.GetSMTPAccount(_domainName);
            if (smtp == null)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmtp") };
            }

            if (_sendSmsConfig == null)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmsSetting") };
            }

            List<MessageTemplate> messageTemplates = await _messageTemplateRepository.GetAllByName(templateName);

            if (!messageTemplates.Any())
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundMessageTemplate") };
            }

            List<NotificationType> notificationTypes = new() { NotificationType.Email };
            List<Notification> notifications = new();

            foreach (NotificationType notificationType in notificationTypes)
            {
                MessageTemplate messageTemplate = messageTemplates.FirstOrDefault(m => m.NotificationType == notificationType
                    && m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

                if (messageTemplate == null)
                {
                    continue;
                }

                string title = GenerateText(notificationType, messageTemplate.MessageTemplateMultiLingual
                    .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, user);
                string messageText = GenerateText(notificationType,
                    messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body, user, callbackUrl);

                Notification notification = new()
                {
                    NotificationId = Guid.NewGuid().ToString(),
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
                    AssociatedDomainId = _domainRepository.FetchByName(_domainName, true).ReturnValue.DomainId
                };

                switch (notificationType)
                {
                    case NotificationType.Email:
                        notification.SMTP = smtp;
                        notification.UserEmail = user.Email;

                        break;

                    case NotificationType.Sms:
                        notification.SendSmsConfig = _sendSmsConfig;
                        notification.UserPhoneNumber = user.PhoneNumber;

                        break;
                }

                notifications.Add(notification);
            }

            if (notifications.Any())
            {
                await _notificationRepository.AddRange(notifications);
            }

            return new() { Succeeded = true, Message = Language.GetString("AlertAndMessage_OperationSuccess") };
        }

        public async Task<Result> SendOtp(string templateName, ApplicationUser user, string otp)
        {
            //SMTP smtp = await _smtpRepository.GetDefault();
            _domainName = $"{_httpContextAccessor.HttpContext.Request.Host}";
            SMTP smtp = _domainRepository.GetSMTPAccount(_domainName);

            if (smtp == null)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmtp") };
            }

            if (_sendSmsConfig == null)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmsSetting") };
            }

            List<MessageTemplate> messageTemplates = await _messageTemplateRepository.GetAllByName(templateName);

            if (!messageTemplates.Any())
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundMessageTemplate") };
            }

            List<NotificationType> notificationTypes = new() { NotificationType.Sms };
            List<Notification> notifications = new();

            foreach (NotificationType notificationType in notificationTypes)
            {
                MessageTemplate messageTemplate = messageTemplates.FirstOrDefault(m => m.NotificationType == notificationType
                && m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

                if (messageTemplate == null)
                {
                    continue;
                }

                string title = GenerateText(notificationType, messageTemplate.MessageTemplateMultiLingual
                    .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, user);
                string messageText = GenerateText(
                    notificationType,
                    messageTemplate.MessageTemplateMultiLingual
                    .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body,
                    user,
                    "",
                    otp,
                    "");

                Notification notification = new()
                {
                    NotificationId = Guid.NewGuid().ToString(),
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
                    SendSmsConfig = _sendSmsConfig,
                    AssociatedDomainId = _domainRepository.FetchByName(_domainName, true).ReturnValue.DomainId
                };

                switch (notificationType)
                {
                    case NotificationType.Email:
                        notification.SMTP = smtp;
                        notification.UserEmail = user.Email;

                        break;

                    case NotificationType.Sms:
                        notification.SendSmsConfig = _sendSmsConfig;
                        notification.UserPhoneNumber = user.PhoneNumber;

                        break;
                }

                notifications.Add(notification);
            }

            if (notifications.Any())
            {
                await _notificationRepository.AddRange(notifications);
            }

            return new() { Succeeded = true, Message = Language.GetString("AlertAndMessage_OperationSuccess") };
        }

        public async Task<Result> SendOtp(string templateName, string phoneNumber, string otp)
        {
            try
            {
                //SMTP smtp = await _smtpRepository.GetDefault();
                _domainName = $"{_httpContextAccessor.HttpContext.Request.Host}";
                SMTP smtp = _domainRepository.GetSMTPAccount(_domainName);

                if (smtp == null)
                {
                    return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmtp") };
                }

                if (_sendSmsConfig == null)
                {
                    return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundSmsSetting") };
                }

                List<MessageTemplate> messageTemplates = await _messageTemplateRepository.GetAllByName(templateName);

                if (!messageTemplates.Any())
                {
                    return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_NotFoundMessageTemplate") };
                }

                List<NotificationType> notificationTypes = new() { NotificationType.Sms };
                List<Notification> notifications = new();

                foreach (NotificationType notificationType in notificationTypes)
                {
                    MessageTemplate messageTemplate = messageTemplates.FirstOrDefault(m => m.NotificationType == notificationType
                    && m.MessageTemplateMultiLingual.Any(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)));

                    if (messageTemplate == null)
                    {
                        continue;
                    }

                    string title = GenerateText(notificationType,
                        messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, null);
                    string messageText = GenerateText
                        (notificationType,
                        messageTemplate.MessageTemplateMultiLingual
                        .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body,
                        null,
                        null,
                        otp,
                        null);

                    ApplicationUser user = _userManager.Users.FirstOrDefault(_ => _.IsSystemAccount);

                    Notification notification = new()
                    {
                        NotificationId = Guid.NewGuid().ToString(),
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
                        SendSmsConfig = _sendSmsConfig,
                        AssociatedDomainId = _domainRepository.FetchByName(_domainName, true).ReturnValue.DomainId
                    };

                    switch (notificationType)
                    {
                        case NotificationType.Email:
                            notification.SMTP = smtp;
                            notification.UserEmail = user.Email;

                            break;

                        case NotificationType.Sms:
                            notification.SendSmsConfig = _sendSmsConfig;
                            notification.UserPhoneNumber = phoneNumber;

                            break;
                    }

                    notifications.Add(notification);
                }

                if (notifications.Any())
                {
                    await _notificationRepository.AddRange(notifications);
                }

                return new() { Succeeded = true, Message = Language.GetString("AlertAndMessage_OperationSuccess") };
            }
            catch (Exception ex)
            {
                return new() { Succeeded = false, Message = Language.GetString("AlertAndMessage_Error") };
            }

        }

        private string GenerateText(NotificationType notificationType, string text,
               ApplicationUser user, string callbackUrl = "", string otp = "", string passwordBeforeHash = "", string productName = "")
        {
            _domainName = $"{_httpContextAccessor.HttpContext.Request.Host}";
            if (string.IsNullOrWhiteSpace(text))
            {
                return String.Empty;
            }

            if (notificationType == NotificationType.Sms)
            {
                text = text.ClearHtml();
                text = text.Replace(@"\n", Environment.NewLine);
            }

            text = text.Replace("{$date}", CultureInfo.CurrentCulture.Name == "fa-IR" ? DateTime.Now.ToPersianLetDateTime() : DateTime.Now.ToString(_setting.DateFormat));
            text = text.Replace("{$time}", DateTime.Now.ToString("HH:mm"));
            text = text.Replace("{$otp}", IsNullOrEmpty(otp));
            text = text.Replace("{$domainName}", _domainName);
            text = text.Replace("{$productName}", productName);


            if (!string.IsNullOrWhiteSpace(callbackUrl))
            {
                string url = $"<a href='{callbackUrl}'>{Language.GetString("Action_Confirmation")}</a>";
                text = text.Replace("{$client_email_verification_link}", url);
            }

            if (user != null)
            {
                text = text.Replace("{$client_id}", user.Id);
                text = text.Replace("{$client_name}", user.UserName);
                text = text.Replace("{$client_first_name}", IsNullOrEmpty(user.Profile.FirstName));
                text = text.Replace("{$client_last_name}", IsNullOrEmpty(user.Profile.LastName));


                text = text.Replace("{$client_email}", IsNullOrEmpty(user.Email));
                text = text.Replace("{$client_signup_date}", CultureInfo.CurrentCulture.Name == "fa-IR" ? user.CreationDate.ToPersianLetDateTime() : user.CreationDate.ToString(_setting.DateFormat));
                text = text.Replace("{$client_phonenumber}", IsNullOrEmpty(user.PhoneNumber));
                text = text.Replace("{$client_status}", user.IsActive ? Language.GetString("Action_Active") : Language.GetString("Action_Inactive"));
                text = text.Replace("{$client_password}", passwordBeforeHash);


            }

            return text;

            string IsNullOrEmpty(string value) => string.IsNullOrWhiteSpace(value) ? "" : value;
        }

    }
}
