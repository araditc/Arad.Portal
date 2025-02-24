using System.Globalization;
using System.Text;

using Arad.Portal.DataLayer.Entities.General.Domain;
using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Entities.General.Language;
using Arad.Portal.DataLayer.Entities.General.Notify;
using Arad.Portal.DataLayer.Entities.General.SendMessage;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Domain;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Language;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Notification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SendMessage;

using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit;

using Newtonsoft.Json;

namespace NotificationService.Services;

public class EmailSenderService(
    INotificationRepository notificationRepository,
    ISendMessageRepository sendMessageRepository,
    IDomainRepository domainRepository,
    ILanguageRepository languageRepository,
    ILogger<EmailSenderService> logger)
    : IDisposable
{
    private bool _isRunning;

    public void Dispose()
    {
    }

    public async Task StartTimer(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {

            logger.LogInformation("Starting Email sender service.");

            if (!_isRunning)
            {
                _isRunning = true;
                await ReadAndSend(stoppingToken);
                _isRunning = false;
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }

    private async Task ReadAndSend(CancellationToken cancellationToken)
    {
        List<Notification>? notifications = await GetPendingNotifications();

        if (notifications != null && !notifications.Any())
        {
            logger.LogInformation("No email to send.");

            return;
        }

        if (notifications != null)
        {
            foreach (Notification notification in notifications)
            {
                try
                {
                    await SendEmailNotification(notification, default);
                    await UpdateNotificationStatus(notification, Enums.NotificationSendStatus.Posted, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error sending email ");
                    await UpdateNotificationStatus(notification, Enums.NotificationSendStatus.Error, cancellationToken);
                }
            }
        }
    }

    private async Task<List<Notification>?> GetPendingNotifications()
    {
        try
        {
            return await notificationRepository.GetListAsync(n =>
                                                                 (n.SendStatus == Enums.NotificationSendStatus.Store) &&
                                                                 n.ScheduleDate <= DateTime.UtcNow &&
                                                                 n.Type == Enums.NotificationType.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving email");

            return null;
        }
    }

    private async Task SendEmailNotification(Notification notification, CancellationToken cancellationToken)
    {
        SendMessage? sendMessage = await sendMessageRepository.FirstOrDefaultAsync(c => c.SendType == SendType.Email && c.AssociatedDomainId == notification.AssociatedDomainId, cancellationToken);
        Smtp? smtpConfig = JsonConvert.DeserializeObject<Smtp>(sendMessage.SendMessageMetaData.ToString() ?? string.Empty);
        MimeMessage message = new();
        Domain domain = await domainRepository.FirstOrDefaultAsync(c => c.Id == notification.AssociatedDomainId, cancellationToken);
        Language language = await languageRepository.FirstOrDefaultAsync(c => c.Symbol == CultureInfo.CurrentCulture.Name, cancellationToken);
        string? title = domain.Titles.FirstOrDefault(c => c.LanguageId == language.Id)?.Name;
        string domainName = !string.IsNullOrEmpty(title) ? title : domain.DomainName;

        // Setting From, To, Subject, and Body
        message.From.Add(new MailboxAddress(Encoding.UTF8, domainName , smtpConfig?.UserName));
            message.To.Add(new MailboxAddress(Encoding.UTF8, notification.UserFullName, notification.UserEmail));
            message.Subject = notification.Title;

            BodyBuilder bodyBuilder = new() { HtmlBody = notification.Body };
            message.Body = bodyBuilder.ToMessageBody();
        

        try
        {
            using SmtpClient emailClient = new();

            // Use the proper SecureSocketOptions when connecting
            if (smtpConfig?.Port != null)
            {
                int port = int.Parse(smtpConfig?.Port);
                SecureSocketOptions secureSocketOptions = port == 587 ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect;

                // Connect using the correct SecureSocketOptions
                await emailClient.ConnectAsync(smtpConfig.Server, port, secureSocketOptions, cancellationToken);
            }

            // Remove XOAUTH2 if not supported
            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

            // Authenticate with SMTP credentials
            await emailClient.AuthenticateAsync(smtpConfig?.UserName, smtpConfig?.Password, cancellationToken);

            // Send the email
            await emailClient.SendAsync(message, cancellationToken);

            // Disconnect from the SMTP server
            await emailClient.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log or handle the error
            logger.LogError(ex, "Failed to send email notification.");
            throw;
        }
    }

    private async Task UpdateNotificationStatus(Notification notification, Enums.NotificationSendStatus status, CancellationToken cancellationToken)
    {
        notification.SendStatus = status;
        notification.SentDate = DateTime.UtcNow;
        await notificationRepository.UpdateAsync(notification, cancellationToken);
    }
}