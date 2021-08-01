using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.ProductUnit
{
    public class ProductUnit : BaseEntity
    {

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ProductUnitId { get; set; }

        public string UnitName { get; set; }
        public string LanguageId { get; set; }
        public string LanguageName { get; set; }
    }
}
