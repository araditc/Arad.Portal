using Arad.Portal.DataLayer.Entities.Shop.ProductSpecification;
using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ProductSpecification
{
    public class ProductSpecificationDTO
    {
        public ProductSpecificationDTO()
        {
            SpecificationNameValues = new List<MultiLingualProperty>();
        }

        public string ProductSpecificationId { get; set; }

        public string SpecificationGroupId { get; set; }
        /// <summary>
        /// languageId, specificationGroupName, specificationName and list of its values
        /// </summary>
        public List<MultiLingualProperty> SpecificationNameValues { get; set; }

        public bool IsDeleted { get; set; }

        public ControlType ControlType { get; set; }

        public string ModificationReason { get; set; }

        public string AssociatedDomainId { get; set; }
    }
}
