using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductUnitDTO 
    {
        public string ProductUnitId { get; set; }

        public string ProductUnitName { get; set; }

        public bool IsEditView { get; set; }
        public string ModificationReason { get; set; }
    }
}
