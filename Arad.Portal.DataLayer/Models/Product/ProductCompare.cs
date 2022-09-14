using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductCompare
    {
        public ProductCompare()
        {
            Specifications = new();
        }
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public long ProductCode { get; set; }

        public decimal CurrentPrice { get; set; }

        public string FormattedPrice { get; set; }

        public string ProductImageUrl { get; set; }

        public List<ProductSpecificationValue> Specifications { get; set; }
    }
}
