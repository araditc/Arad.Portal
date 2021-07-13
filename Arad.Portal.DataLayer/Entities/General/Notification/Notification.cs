using Arad.Portal.DataLayer.Models.Attachment;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Entities.General.Notification
{
    public class Notification : BaseEntity
    {
        public Notification()
        {
            Attachments = new();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string NotificationId { get; set; }

        public NotificationType Type { get; set; }

        public DateTime ScheduleDate { get; set; }

        public string To { get; set; }

        public string From { get; set; }

        public string Title { get; set; }

        public string MessageText { get; set; }

        public string UpStreamGatewayId { get; set; }

        public List<Attachment> Attachments { get; set; }

        public NotificationSendStatus SendStatus { get; set; }

        public DateTime SentDate { get; set; }

        public string TemplateName { get; set; }
    }
}
