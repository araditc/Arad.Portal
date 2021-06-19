using Arad.Portal.DataLayer.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.ProductSpecification
{
    public class ProductSpecification
    {

        public string ProductSpecificationId { get; set; }

        public SpecificationGroup SpecificationGroup { get; set; }

        public string SpecificationName { get; set; }

        public string SpecificationValue { get; set; }
    }
}
