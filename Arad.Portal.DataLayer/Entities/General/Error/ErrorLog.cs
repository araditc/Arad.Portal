using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Error
{
    public class ErrorLog : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ErrorLogId { get; set; }

        public string Source { get; set; }

        public string Error { get; set; }

        public string Ip { get; set; }
    }
}
