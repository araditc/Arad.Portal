using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ShoppingCart
{
    public class PurchasePerSellerDTO 
    {
        public PurchasePerSellerDTO()
        {
            Products = new();
        }
        public string SellerId { get; set; }

        public string SellerUserName { get; set; }

        public List<ShoppingCartDetailDTO> Products { get; set; }

        public int ShippingTypeId { get; set; }

        public long ShippingExpense { get; set; }

        public long TotalDetailsAmountWithShipping { get; set; }
    }
}
