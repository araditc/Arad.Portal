using Arad.Portal.DataLayer.Models.Shared;
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
        public ProductUnit()
        {
            UnitNames = new();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ProductUnitId { get; set; }
        public List<MultiLingualProperty> UnitNames { get; set; }

    }
}
