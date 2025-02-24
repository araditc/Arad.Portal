using System;
using System.Collections.Generic;
using System.Text.Json;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Entities.General.SendMessage;
using Arad.Portal.DataLayer.Entities.General.SMS;
using Arad.Portal.DataLayer.Models.Shared.Attachment;
using Arad.Portal.GeneralLibrary.CustomAttributes;

using MongoDB.Bson.Serialization.Attributes;

using static Arad.Portal.DataLayer.Models.Shared.Enums;

using SendType = Arad.Portal.DataLayer.Entities.General.SendMessage.SendType;

namespace Arad.Portal.DataLayer.Entities.General.Notify;

public class Notification : BaseEntity
{
    public NotificationType Type { get; init; }

    public ActionType ActionType { get; init; }

    public NotificationSendStatus SendStatus { get; set; }

    public DateTime SentDate { get; set; }

    public DateTime ScheduleDate { get; init; }

    public string Title { get; init; }

    public string Body { get; init; }

    public SendType SendType { get; init; }

    public Provider Provider { get; init; }

    [BsonSerializer(typeof(CustomSerializer))]
    public object SendMessageMetaData { get; set; }

    /// <summary>
    ///     The fullname (FirstName + ' '+ LastName) of user who is the creator of this notification
    /// </summary>
    public string UserFullName { get; set; }

    public string UserEmail { get; set; }

    public string UserPhoneNumber { get; set; }

    public List<Attachment> Attachments { get; init; } = [];

    public List<(string Key, string Value)> ExtraData { get; init; } = [];

    public string TemplateName { get; init; }

    // Copy method to create a new instance of SendMessage
    public SendMessage.SendMessage Copy()
    {
        SendMessage.SendMessage copy = (SendMessage.SendMessage)MemberwiseClone();

        if (SendMessageMetaData is string metadataStr)
        {
            // Deserialize only if it's a string
            copy.SendMessageMetaData = SendType switch
                                       {
                                           SendType.Email => JsonSerializer.Deserialize<Smtp>(metadataStr),
                                           SendType.SMS => JsonSerializer.Deserialize<Sms>(metadataStr),
                                           _ => copy.SendMessageMetaData
                                       };
        }
        else
        {
            // If already an object, just clone it
            copy.SendMessageMetaData = SendMessageMetaData;
        }

        return copy;
    }
}