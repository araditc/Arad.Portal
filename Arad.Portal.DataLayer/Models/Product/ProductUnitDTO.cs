using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductUnitDTO 
    {
        public ProductUnitDTO()
        {
            UnitNames = new List<MultiLingualProperty>();
        }
        public string ProductUnitId { get; set; }
       
        public List<MultiLingualProperty> UnitNames { get; set; }
        public string ModificationReason { get; set; }
        public bool IsDeleted { get; set; }
    }
}
