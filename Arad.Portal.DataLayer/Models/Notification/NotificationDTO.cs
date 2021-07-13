using System;
using System.Collections.Generic;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Models.Notification
{
    public class NotificationDTO 
    {
        public NotificationDTO()
        {
            Attachments = new();
        }

        public string NotificationId { get; set; }

        public NotificationType Type { get; set; }

        public DateTime ScheduleDate { get; set; }

        public string To { get; set; }

        public string From { get; set; }

        public string Title { get; set; }

        public string MessageText { get; set; }

        public string UpStreamGatewayId { get; set; }

        public List<Attachment.Attachment> Attachments { get; set; }

        public NotificationSendStatus SendStatus { get; set; }

        public DateTime SentDate { get; set; }

        public string TemplateName { get; set; }

        public DateTime CreationDate { get; set; }

        public string CreatorUserId { get; set; }

        public string CreatorUserName { get; set; }
    }
}
