using Arad.Portal.DataLayer.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ProductGroup
{
    public class ProductGroupViewList
    { 
        public ProductGroupViewList()
        {
            Childs = new();
            Products = new();
        }
        public List<ProductGroupDTO> Childs { get; set; }

        public List<ProductOutputDTO> Products { get; set; }
    }
}
