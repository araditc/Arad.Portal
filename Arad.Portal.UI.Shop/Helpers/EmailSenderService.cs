using Arad.Portal.DataLayer.Contracts.General.Notification;
using Arad.Portal.DataLayer.Entities.General.Notify;
using Arad.Portal.DataLayer.Helpers;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Hosting;
using MimeKit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;
namespace Arad.Portal.UI.Shop.Helpers
{
    public class EmailSenderService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly CreateNotification _createNotification;
        private readonly Setting _setting;
        private Timer _timer;
        private bool _flag = true;
        public EmailSenderService(INotificationRepository notificationRepository, Setting setting, CreateNotification createNotification)
        {
            _notificationRepository = notificationRepository;
            _setting = setting;
            _createNotification = createNotification;
        }

        public void StartTimer()
        {
            TimerCallback cb = new(OnTimeEvent);
            _timer = new(OnTimeEvent, null, 1000, 10000);
            Log.Fatal("*************************STTTTTTTttart Email service timer*************************");
        }

        private async void OnTimeEvent(object state)
        {
           
            if (_flag)
            {
                await ReadAndSend();
            }
        }

        private async Task ReadAndSend()
        {
            _flag = false;

            Stopwatch sw1 = new();
            Stopwatch sw2 = new();
            int sucessCount = 0;
            int failedCount = 0;
            sw1.Start();
            List<Notification> wholeList = await _notificationRepository.GetForSend(NotificationType.Email);
            List<Notification> emailNotifications = wholeList.Where(_ => _.ActionType == ActionType.NoExtraAction).ToList();
            List<Notification> productNotification = wholeList.Where(_ => _.ActionType == ActionType.ProductAvailibilityReminder).ToList();
            sw1.Stop();
            sw2.Start();

            if (emailNotifications.Any())
            {
                foreach (Notification notification in emailNotifications)
                {
                    try
                    {
                        MimeMessage message = new();
                        message.From.Add(new MailboxAddress(notification.SMTP.DisplayName, notification.SMTP.EmailAddress));
                        message.To.Add(new MailboxAddress(notification.UserFullName, notification.UserEmail));

                        message.Subject = notification.Title;

                        BodyBuilder bodyBuilder = new() { HtmlBody = notification.Body };
                        message.Body = bodyBuilder.ToMessageBody();

                        using SmtpClient emailClient = new();
                        await emailClient.ConnectAsync(notification.SMTP.Server, notification.SMTP.ServerPort,
                              notification.SMTP.IgnoreSSLWarning);
                        emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                        await emailClient.AuthenticateAsync(notification.SMTP.SMTPAuthUsername, notification.SMTP.SMTPAuthPassword);
                        await emailClient.SendAsync(message);
                        await emailClient.DisconnectAsync(true);

                        notification.SendStatus = NotificationSendStatus.Posted;
                        notification.SentDate = DateTime.Now;
                        await _notificationRepository.Update(notification, "", true);
                        sucessCount++;
                    }
                    catch (Exception e)
                    {
                        failedCount++;
                        notification.SendStatus = NotificationSendStatus.Error;
                        notification.ScheduleDate = DateTime.Now.AddMinutes(5);
                        await _notificationRepository.Update(notification, e.Message, true);
                        Logger.WriteLogFile($"Error in sending email. Error is: {e.Message}");
                    }
                }
            }
            foreach (var notify in productNotification)
            {
                await _createNotification.GenerateProductNotificationToUsers("ProductAvailibilityNotify", notify);
            }

            sw2.Stop();
            Logger.WriteLogFile($"RowCount: {emailNotifications.Count}\t " +
                               $"ReadTime: {sw1.ElapsedMilliseconds}\t " +
                               $"SendTime: {sw2.ElapsedMilliseconds}\t " +
                               $"SuccessCount: {sucessCount}\t " +
                               $"FailedCount: {failedCount}");
            _flag = true;
        }
    }
}
