using System.Text.Json;

using Arad.Portal.DataLayer.Entities.Abstractions;
using Arad.Portal.DataLayer.Entities.General.Email;
using Arad.Portal.DataLayer.Entities.General.SMS;
using Arad.Portal.GeneralLibrary.CustomAttributes;

using MongoDB.Bson.Serialization.Attributes;

namespace Arad.Portal.DataLayer.Entities.General.SendMessage;

public class SendMessage : BaseEntity
{
    public SendType SendType { get; set; }
    public Provider Provider { get; set; }

    [BsonSerializer(typeof(CustomSerializer))]
    public object SendMessageMetaData { get; set; }

    public SendMessage Copy()
    {
        SendMessage copy = (SendMessage)MemberwiseClone();

        if (SendMessageMetaData is string metadataStr)
        {
            copy.SendMessageMetaData = SendType switch
                                       {
                                           SendType.Email => JsonSerializer.Deserialize<Smtp>(metadataStr),
                                           SendType.SMS => JsonSerializer.Deserialize<Sms>(metadataStr),
                                           _ => copy.SendMessageMetaData
                                       };
        }
        else
        {
            copy.SendMessageMetaData = SendMessageMetaData;
        }

        return copy;
    }
}

public enum Provider
{
    Arad
}

public enum SendType
{
    Email = 0,
    SMS = 1
}