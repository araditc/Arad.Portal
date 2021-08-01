using Arad.Portal.DataLayer.Models.Product;
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
            SpecificationValues = new ();
        }

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ProductSpecificationId { get; set; }

        public string SpecificationGroupId { get; set; }

        public string SpecificationGroupName { get; set; }

        public string SpecificationName { get; set; }

        /// <summary>
        /// all possible values which this specification has
        /// </summary>
        public List<string> SpecificationValues { get; set; }
    }
}
