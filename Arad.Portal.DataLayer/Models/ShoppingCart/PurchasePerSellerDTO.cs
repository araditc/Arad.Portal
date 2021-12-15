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
        public string SellerId { get; set; }

        public string SellerUserName { get; set; }

        public List<ShoppingCartDetailDTO> Products { get; set; }

        public ShippingType ShippingType { get; set; }

        public decimal ShippingExpense { get; set; }

        public decimal TotalDetailsAmountWithShipping { get; set; }
    }
}
