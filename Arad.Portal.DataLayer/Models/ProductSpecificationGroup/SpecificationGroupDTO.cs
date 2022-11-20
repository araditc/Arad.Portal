using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ProductSpecificationGroup
{
    public class SpecificationGroupDTO
    {
        public SpecificationGroupDTO()
        {
            GroupNames = new List<MultiLingualProperty>();
        }
        public string SpecificationGroupId { get; set; }
        public List<MultiLingualProperty> GroupNames { get; set; }
        public string ModificationReason { get; set; }
        public bool IsDeleted { get; set; }
        public string AssociatedDomainId { get; set; }
    }
}
