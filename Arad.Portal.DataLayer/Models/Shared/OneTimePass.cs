using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class OneTimePass
    {
        public string Content { get; set; }


        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ExpirationDate { get; set; }
    }
}
