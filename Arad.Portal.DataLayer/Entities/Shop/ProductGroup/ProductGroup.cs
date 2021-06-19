using Arad.Portal.DataLayer.Entities.Shop.Product;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.ProductGroup
{
    public class ProductGroup
    {
        public ProductGroup()
        {
            MultiLingualProperties = new List<MultiLingualProperty>();
        }
        public string Id { get; set; }

        public List<MultiLingualProperty> MultiLingualProperties { get; set; }

        public string ParentId { get; set; } 
        
        public int IsDeleted { get; set; }

        public Promotion.Promotion Promotion { get; set; }

    }
}
