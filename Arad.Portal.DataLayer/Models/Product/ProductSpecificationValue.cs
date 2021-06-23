using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductSpecificationValue
    {
        public Entities.Shop.ProductSpecification.ProductSpecification Specification { get; set; }

        public string Value { get; set; }
    }
}
