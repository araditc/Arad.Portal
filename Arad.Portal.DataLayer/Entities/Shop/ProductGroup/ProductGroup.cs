using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.ProductGroup
{
    public class ProductGroup : BaseEntity
    {
        public ProductGroup()
        {
            MultiLingualProperties = new List<MultiLingualProperty>();
            GroupImage = new Image();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ProductGroupId { get; set; }

        public List<MultiLingualProperty> MultiLingualProperties { get; set; }

        public long GroupCode { get; set; }

        public string ParentId { get; set; }

        public Image GroupImage { get; set; }

        public Promotion.Promotion Promotion { get; set; }

    }
}
