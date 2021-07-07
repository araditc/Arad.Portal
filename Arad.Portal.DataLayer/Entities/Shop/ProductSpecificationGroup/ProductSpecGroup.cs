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

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string SpecificationGroupId { get; set; }
        public string GroupName { get; set; }
        public string LanguageId { get; set; }
        public string LanguageName { get; set; }
    }
}
