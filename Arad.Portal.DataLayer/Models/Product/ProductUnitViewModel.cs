using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductUnitViewModel
    {
        public ProductUnitViewModel()
        {
            UnitName = new MultiLingualProperty();
        }
        public string ProductUnitId { get; set; }

        public bool IsDeleted { get; set; }

        public MultiLingualProperty UnitName { get; set; }
    }
}
