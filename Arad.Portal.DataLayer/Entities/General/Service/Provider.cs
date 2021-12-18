using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Service
{
    public class Provider : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ProviderId { get; set; }

        public ProviderType ProviderType { get; set; }

        public string ProviderName { get; set; }

        public string JsonContent { get; set; }
    }

    public enum ProviderType
    {
       Shipping,
       Payment,
       SMS
       //,
       //etc
    }
}
