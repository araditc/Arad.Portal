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
using Arad.Portal.DataLayer.Contracts.General.Email;
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
        private readonly IWebHostEnvironment _env;
        private readonly string _domainName;

        public CreateNotification(IMessageTemplateRepository messageTemplateRepository,
                                  ISystemSettingRepository systemSettingRepository,
                                  INotificationRepository notificationRepository,
                                  //ISMTPRepository smtpRepository,
                                  UserManager<ApplicationUser> userManager,
                                  IHttpContextAccessor httpContextAccessor,
                                  Setting setting,
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
            _domainRepository = domainRepository;
            //if(_env.EnvironmentName !=  "Development")
            //{
                _domainName = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
            //}
            //else
            //{
            //    _domainName = "http://localhost:17951";
            //}
            
        }



        public async Task<Result> ClientSignUp(string templateName, ApplicationUser user, string password)
        {
            
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
                
                string title = await GenerateText(notificationType,
                    messageTemplate.MessageTemplateMultiLingual
                    .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject,  user);
                string messageText = await GenerateText(notificationType, messageTemplate.MessageTemplateMultiLingual
                    .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body,  user, "", "", password);

                Notification notification = new()
                {
                    Type = notificationType,
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
                    AssociatedDomainId = _domainRepository.FetchByName(_domainName).ReturnValue.DomainId
                };

                switch (notificationType)
                { 
                    case NotificationType.Email:
                        notification.SMTP = smtp;
                        //todo notification.UserEmail = user.Email;

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

        public async Task<Result> SendCustomMessage(ApplicationUser user, string messageText)
        {
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
                    Body = messageText,
                    CreationDate = DateTime.Now.ToUniversalTime(),
                    CreatorUserId = user.Id,
                    CreatorUserName = user.UserName,
                    TemplateName = "",
                    UserFullName = "",
                    SendSmsConfig = _sendSmsConfig,
                    AssociatedDomainId = _domainRepository.FetchByName(_domainName).ReturnValue.DomainId
                };

                switch (notificationType)
                {
                    case NotificationType.Email:
                        //notification.SMTP = smtp;
                        //todo notification.UserEmail = user.Email;

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

                string title = await GenerateText(notificationType, 
                    messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, user);
                string messageText = await GenerateText(notificationType, 
                    messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body,  user, "", "", password);

                Notification notification = new()
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    IsActive = true,
                    Type = notificationType,
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
                    AssociatedDomainId = _domainRepository.FetchByName(_domainName).ReturnValue.DomainId
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

                string title = await GenerateText(notificationType, messageTemplate.MessageTemplateMultiLingual
                    .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, user);
                string messageText = await GenerateText(notificationType,
                    messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Body,  user, callbackUrl);
                
                Notification notification = new()
                {
                    NotificationId = Guid.NewGuid().ToString(),
                    IsActive = true,
                    Type = notificationType,
                    ScheduleDate = DateTime.Now.ToUniversalTime(),
                    SendStatus = NotificationSendStatus.Store,
                    Title = title,
                    Body = messageText,
                    CreationDate = DateTime.Now.ToUniversalTime(),
                    CreatorUserId = user.Id,
                    CreatorUserName = user.UserName,
                    TemplateName = templateName,
                    UserFullName = user.Profile.FullName,
                    AssociatedDomainId = _domainRepository.FetchByName(_domainName).ReturnValue.DomainId
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

                string title = await GenerateText(notificationType, messageTemplate.MessageTemplateMultiLingual
                    .First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, user);
                string messageText = await GenerateText(
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
                    AssociatedDomainId = _domainRepository.FetchByName(_domainName).ReturnValue.DomainId
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

                    string title = await GenerateText(notificationType,
                        messageTemplate.MessageTemplateMultiLingual.First(d => d.LanguageName.Equals(CultureInfo.CurrentCulture.Name)).Subject, null);
                    string messageText = await GenerateText
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
                        AssociatedDomainId = _domainRepository.FetchByName(_domainName).ReturnValue.DomainId
                    };

                    switch (notificationType)
                    {
                        case NotificationType.Email:
                            notification.SMTP = smtp;
                            //todo notification.UserEmail = user.Email;

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
                return new() {Succeeded = false , Message = Language.GetString("AlertAndMessage_Error") };
            }
            
        }

        private async Task<string> GenerateText(NotificationType notificationType, string text,
               ApplicationUser user, string callbackUrl = "", string otp = "", string passwordBeforeHash = "")
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            if (notificationType == NotificationType.Sms)
            {
                text = text.ClearHtml();
                text = text.Replace(@"\n", Environment.NewLine);
            }

            text = text.Replace("{$date}", CultureInfo.CurrentCulture.Name == "fa-IR" ? DateTime.Now.ToPersianLetDateTime() : DateTime.Now.ToString(_setting.DateFormat));
            text = text.Replace("{$time}", DateTime.Now.ToString("HH:mm"));
            text = text.Replace("{$otp}", IsNullOrEmpty(otp));
            text = text.Replace("{$company_name}", _domainName);


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

                //if (user.UserLegal != null)
                //{
                //    text = text.Replace("{$client_company_name}", IsNullOrEmpty(user.UserLegal.Name));
                //    text = text.Replace("{$client_tax_id}", IsNullOrEmpty(user.UserLegal.TaxNo));
                //}

                text = text.Replace("{$client_email}", IsNullOrEmpty(user.Email));
                text = text.Replace("{$client_signup_date}", CultureInfo.CurrentCulture.Name == "fa-IR" ? user.CreationDate.ToPersianLetDateTime() : user.CreationDate.ToString(_setting.DateFormat));
                text = text.Replace("{$client_phonenumber}", IsNullOrEmpty(user.PhoneNumber));
                //text = text.Replace("{$client_group_id}", user.DefaultGroup.Id);
                //text = text.Replace("{$client_group_name}", user.DefaultGroup.GroupName);
                text = text.Replace("{$client_due_invoices_balance}", "0"); //TODO {$client_due_invoices_balance}
                text = text.Replace("{$client_status}", user.IsActive ? Language.GetString("Action_Active") : Language.GetString("Action_Inactive"));
                //text = text.Replace("{$signature}", user.Profile.Signature);
                text = text.Replace("{$client_password}", passwordBeforeHash);

                //if (user.Addresses != null)
                //{
                //    text = text.Replace("{$client_address1}", IsNullOrEmpty(user.Address.MainAddress));
                //    text = text.Replace("{$client_city}", IsNullOrEmpty(user.Address.CityName));
                //    text = text.Replace("{$client_state}", IsNullOrEmpty(user.Address.StateName));
                //    text = text.Replace("{$client_postcode}", IsNullOrEmpty(user.Address.PostalCode));
                //    text = text.Replace("{$client_country}", IsNullOrEmpty(user.Address.CountyName));
                //}
            }

            //SystemSetting setting = (await _systemSettingRepository.GetAll()).FirstOrDefault();

            //if (setting != null)
            //{
            //text = text.Replace("{$company_name}", IsNullOrEmpty(setting.CompanyName));
            //text = text.Replace("{$company_logo_url}", IsNullOrEmpty(setting.CompanyLogoUrl));
            //text = text.Replace("{$company_domain}", IsNullOrEmpty(setting.CompanyDomain));
            //text = text.Replace("{$company_tax_code}", IsNullOrEmpty(setting.CompanyTaxId));

            //if (!string.IsNullOrWhiteSpace(setting.CompanyDomain))
            //{
            //    string url = $"<a href='{setting.CompanyDomain}'>{setting.CompanyName}</a>";
            //    text = text.Replace("{$company_domain}", url);
            //}

            //if (!string.IsNullOrWhiteSpace(setting.CompanyLogoUrl))
            //{
            //    string url = $"<img src='{setting.CompanyLogoUrl}'/>";
            //    text = text.Replace("{$company_logo_url}", url);
            //}
            //}
            
            return text;

            string IsNullOrEmpty(string value) => string.IsNullOrWhiteSpace(value) ? "" : value;
        }

    }
}
