using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Arad.Portal.DataLayer.Entities.General.Currency
{
    /// <summary>
    /// only SysAccount user can define currency
    /// </summary>
    public class Currency : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        /// <summary>
        /// each currency has a prefix that define that currency it is that prefix
        /// </summary>
        public string Prefix { get; set; }

        public string Symbol { get; set; }

        public bool IsDefault { get; set; }

    }
}
