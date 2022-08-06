using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.SystemSetting
{
    /// <summary>
    /// this entity store some settings of site owner which cann access in different part of site
    /// </summary>
    public class SystemSetting : BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string SystemSettingId { get; set; }

        public string SystemTitle { get; set; }

        public string SystemIcon { get; set; }

        public string CompanyName { get; set; }

        public string CompanyDomain { get; set; }

        public string CompanyLogoUrl { get; set; }

        public string CompanyTaxId { get; set; }

        public string CompanyAddress { get; set; }

        public string CompanyPhone { get; set; }

        public string CompanyEmail { get; set; }

        public bool MandatoryControlDocumentReview { get; set; }
    }
}
