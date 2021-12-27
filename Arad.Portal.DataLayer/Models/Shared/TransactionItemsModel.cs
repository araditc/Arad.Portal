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
        public List<ProductOrder> Orders { get; set; }
    }
    public class ProductOrder
    {
        public string ProductId { get; set; }

        public string OrderCount { get; set; }
    }
}
