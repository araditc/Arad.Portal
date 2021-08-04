using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ProductSpecification
{
    public class ProductSpecificationViewModel
    {
        public string ProductSpecificationId { get; set; }
        public string SpecificationGroupId { get; set; }
        /// <summary>
        /// languageId, specificationGroupName, specificationName and list of its values
        /// </summary>
        public MultiLingualProperty SpecificationNameValues { get; set; }
        public bool IsDeleted { get; set; }
        public string ModificationReason { get; set; }
    }
}
