using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.ProductSpecification;
using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.ProductSpecification
{
    public class ProductSpecification : BaseEntity
    {
        public ProductSpecification()
        {
            SpecificationNameValues = new List<MultiLingualProperty>();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ProductSpecificationId { get; set; }

        public string SpecificationGroupId { get; set; }
        /// <summary>
        /// languageId, groupname, specificationName and list of values
        /// </summary>
        public List<MultiLingualProperty> SpecificationNameValues { get; set; }
    }
}
