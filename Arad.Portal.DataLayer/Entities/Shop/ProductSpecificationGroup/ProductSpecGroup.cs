using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.ProductSpecificationGroup
{
    public class ProductSpecGroup : BaseEntity
    {
        public ProductSpecGroup()
        {
            GroupNames = new List<MultiLingualProperty>();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string SpecificationGroupId { get; set; }
        public List<MultiLingualProperty> GroupNames { get; set; }
    }
}
