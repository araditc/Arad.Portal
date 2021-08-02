using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.DataLayer.Models.ProductGroup
{
    public class ProductGroupViewModel
    {
        public ProductGroupViewModel()
        {
            MultiLingualProperty = new MultiLingualProperty();
        }
        public string ProductGroupId { get; set; }

        public MultiLingualProperty MultiLingualProperty { get; set; }

        public string ParentId { get; set; }

        public bool IsDeleted { get; set; }

        public string ModificationReason { get; set; }

        public Entities.Shop.Promotion.Promotion Promotion { get; set; }
    }
}
