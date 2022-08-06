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

        /// <summary>
        /// action which this error occured
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// content of error
        /// </summary>
        public string Error { get; set; }


        /// <summary>
        /// IP where this error occured
        /// </summary>
        public string Ip { get; set; }
    }
}
