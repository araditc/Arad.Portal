using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Models.Attachment;
using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Entities.General.Notify
{
    public class Notification : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string NotificationId { get; set; }

        public NotificationType Type { get; set; }

        public NotificationSendStatus SendStatus { get; set; }

        public DateTime SentDate { get; set; }

        public DateTime ScheduleDate { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public SMTP SMTP { get; set; }

        public SendSmsConfig SendSmsConfig { get; set; }

        public string UserFullName { get; set; }

        public string UserEmail { get; set; }

        public string UserPhoneNumber { get; set; }

        public string UpStreamGatewayId { get; set; }

        public List<Attachment> Attachments { get; set; }

        public string TemplateName { get; set; }
    }
}
