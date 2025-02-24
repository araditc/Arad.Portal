using System.Net;
using System.Text;
using Newtonsoft.Json;
using Arad.Portal.DataLayer.Entities.General.Notify;
using Arad.Portal.DataLayer.Entities.General.SendMessage;
using Arad.Portal.DataLayer.Entities.General.SMS;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.Notification;
using Arad.Portal.DataLayer.Repositories.Interfaces.General.SendMessage;
using Arad.Portal.Shared;

using Serilog;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Web;

namespace NotificationService.Services;

public class SmsSenderService(
    INotificationRepository notificationRepository,
    IHttpClientFactory clientFactory,
    ISendMessageRepository sendMessageRepository,
    ILogger<SmsSenderService> logger)
    : IDisposable
{
    private bool _isRunning;

    public void Dispose()
    {
    }

    public async Task StartTimer(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting SMS sender service.");

        while (!stoppingToken.IsCancellationRequested)
        {

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
            logger.LogInformation("No sms to send.");

            return;
        }

        if (notifications != null)
        {
            foreach (Notification notification in notifications)
            {
                try
                {
                    await SendNotification(notification, default);
                    await UpdateNotificationStatus(notification, Enums.NotificationSendStatus.Posted, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error sending sms.");
                    await UpdateNotificationStatus(notification, Enums.NotificationSendStatus.Error, cancellationToken);
                }

            }
        }
    }

    private async Task<List<Notification>?> GetPendingNotifications()
    {
        try
        {
            return await notificationRepository.GetListAsync(predicate: n =>
                                                                             (n.SendStatus == Enums.NotificationSendStatus.Store) &&
                                                                             n.ScheduleDate <= DateTime.UtcNow &&
                                                                             n.Type == Enums.NotificationType.Sms);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving sms.");

            return null;
        }
    }

    private async Task SendNotification(Notification notification, CancellationToken cancellationToken)
    {
        SendMessage? sendMessageConfig = await sendMessageRepository.FirstOrDefaultAsync(c => (int)c.SendType == 1 && c.AssociatedDomainId == notification.AssociatedDomainId, cancellationToken);

        if (sendMessageConfig.SendType == SendType.SMS)
        {
            Sms smsConfig = JsonConvert.DeserializeObject<Sms>(sendMessageConfig.SendMessageMetaData.ToString());
            SmsRequest modelToSend = new()
            {
                SourceAddress = smsConfig.SenderNumber,
                DestinationAddress = notification.UserPhoneNumber,
                MessageText = notification.Body
            };

            List<SmsRequest> modelToSends = [modelToSend];
            HttpResponseMessage response = await SendSms(modelToSends, smsConfig, cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException($"Failed to send SMS. Status code: {response.StatusCode}");
            }
        }
    }

    private async Task<HttpResponseMessage> SendSms(List<SmsRequest> data, Sms smsConfig, CancellationToken cancellationToken)
    {
        HttpClient client = clientFactory.CreateClient();
        HttpRequestMessage request = new(HttpMethod.Post, $"{smsConfig.BaseAddress}/api/message/send");
        request.Headers.Add("x-api-key", smsConfig.ApiKey);

        StringContent content = new(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        logger.LogInformation("Sending SMS: {data}", JsonConvert.SerializeObject(data));
        request.Content = content;

        HttpResponseMessage response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to send SMS. Status code: {StatusCode}, Response: {Response}",
                            response.StatusCode,
                            await response.Content.ReadAsStringAsync(cancellationToken));
        }

        return response;
    }

    private async Task UpdateNotificationStatus(Notification notification, Enums.NotificationSendStatus status, CancellationToken cancellationToken)
    {
        notification.SendStatus = status;
        notification.SentDate = DateTime.UtcNow;
        await notificationRepository.UpdateAsync(notification, cancellationToken);
    }
}