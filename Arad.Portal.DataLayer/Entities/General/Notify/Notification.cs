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
        public Notification()
        {
            ExtraData = new();
        }
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string NotificationId { get; set; }

        public NotificationType Type { get; set; }

        public ActionType ActionType { get; set; }

        public NotificationSendStatus SendStatus { get; set; }

        public DateTime SentDate { get; set; }

        public DateTime ScheduleDate { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public SMTP SMTP { get; set; }
        /// <summary>
        /// the config of sms line which we use for sending sms
        /// </summary>
        public SmsEndPointConfig SmsEndPointConfig { get; set; }

        /// <summary>
        /// the fullname (FirstName + ' '+ LastName of user who is creator of this notification
        /// </summary>
        public string UserFullName { get; set; }

        public string UserEmail { get; set; }

        public string UserPhoneNumber { get; set; }

        public List<Attachment> Attachments { get; set; }

        public List<(string, string)> ExtraData { get; set; }

        public string TemplateName { get; set; }
    }
}
