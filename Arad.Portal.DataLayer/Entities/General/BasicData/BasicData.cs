using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.BasicData
{
    public class BasicData : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string BasicDataId { get; set; }
        public string GroupKey { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
        public int Order { get; set; }
    }
}
