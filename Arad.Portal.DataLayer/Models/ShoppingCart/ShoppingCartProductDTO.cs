using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ShoppingCart
{
    public class ShoppingCartProductDTO
    {
        public string UserId { get; set; }

        public string UserName { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int InventoryCount { get; set; }
        public int OrderCount { get; set; }
        public long PricePerUnit { get; set; }
    }
}
