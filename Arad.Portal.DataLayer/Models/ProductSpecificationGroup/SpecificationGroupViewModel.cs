using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ProductSpecificationGroup
{
    public class SpecificationGroupViewModel
    {
        public SpecificationGroupViewModel()
        {
            GroupName = new MultiLingualProperty();
        }
        public string SpecificationGroupId { get; set; }
        public MultiLingualProperty GroupName { get; set; }
        public string ModificationReason { get; set; }
        public bool IsDeleted { get; set; }
    }
    
}
