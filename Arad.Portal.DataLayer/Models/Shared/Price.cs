using Arad.Portal.DataLayer.Entities.General.Currency;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Arad.Portal.DataLayer.Models.Shared
{
    public class Price
    {
        public string PriceId { get; set; }

        public string CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        public string Symbol { get; set; }

        public string Prefix { get; set; }

        [BsonRepresentation(BsonType.Decimal128)]
        public decimal PriceValue { get; set; }

        public bool IsActive { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EndDate { get; set; }
    }
}
