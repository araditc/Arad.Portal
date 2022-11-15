using Arad.Portal.DataLayer.Models.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class TransactionItems
    {
        public TransactionItems()
        {
            Orders = new();
        }

        public DateTime CreatedDate { get; set; }
        public List<ProductOrder> Orders { get; set; }
    }
    public class ProductOrder
    {
        public ProductOrder()
        {
            SpecValues = new();
        }
        public string ProductId { get; set; }

        public List<SpecValue> SpecValues { get; set; }

        public int OrderCount { get; set; }
    }
}
