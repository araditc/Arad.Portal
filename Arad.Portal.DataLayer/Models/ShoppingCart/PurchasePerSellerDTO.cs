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

        public List<ShoppingCartDetailDTO> Products { get; set; }

        public TransferType TransferType { get; set; }

        public decimal TransferExpense { get; set; }

        public decimal TotalDetailsAmountWithTransfer { get; set; }
    }
}
