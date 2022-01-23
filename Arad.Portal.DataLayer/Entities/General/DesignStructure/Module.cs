using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.DesignStructure
{
    public class Module : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ModuleId { get; set; }
        public ModuleContentType ModuleContentType { get; set; }
        public string ComponentName { get; set; }
        public List<KeyVal> ParamsWithDefaultValues { get; set; }

    }
    public enum ModuleContentType
    {
        MostPopulareProducts,
        MostVisitedProducts,
        BestSellerProducts,
        NewestProduct,
        ImageSlider,
        MostPopularContent,
        MostVisitedContent,
        NewestContent,
        Advertisement
    }
}
