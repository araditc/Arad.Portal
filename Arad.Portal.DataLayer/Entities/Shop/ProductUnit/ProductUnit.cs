using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.ProductUnit
{
    /// <summary>
    /// product have different unit for counting this as avalable units for product that can be extended
    /// </summary>
    public class ProductUnit : BaseEntity
    {
        public ProductUnit()
        {
            UnitNames = new();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ProductUnitId { get; set; }

        /// <summary>
        /// Language and Name as unitName will be store here
        /// </summary>
        public List<MultiLingualProperty> UnitNames { get; set; }

    }
}
