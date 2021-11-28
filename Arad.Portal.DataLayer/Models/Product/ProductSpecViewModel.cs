using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductSpecViewModel
    {
        public ProductSpecViewModel()
        {
            ProductSpecificationValues = new();
        }
        public Entities.Shop.ProductSpecificationGroup.ProductSpecGroup SpecGroup { get; set; }
        public List<ProductSpecificationValue> ProductSpecificationValues { get; set; }
    }
}
