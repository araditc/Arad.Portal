using Arad.Portal.DataLayer.Entities.General.DesignStructure;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.DesignStructure
{
    public class ModuleParameters
    {
        [BsonIgnoreIfNull]
        public int? Count { get; set; }

        [BsonIgnoreIfNull]
        public ProductOrContentType? ProductOrContentType { get; set; }

        [BsonIgnoreIfNull]
        public ProductTemplateDesign? ProductTemplateDesign { get; set; }

        [BsonIgnoreIfNull]
        public ContentTemplateDesign? ContentTemplateDesign { get; set; }

        [BsonIgnoreIfNull]
        public ImageSliderTemplateDesign? ImageSliderTemplateDesign { get; set; }

        [BsonIgnoreIfNull]
        public AdvertisementTemplateDesign? AdvertisementTemplateDesign { get; set; }

        [BsonIgnoreIfNull]
        public string DomainId { get; set; }

        [BsonIgnoreIfNull]
        public string LanguageId { get; set; }
    }
}
