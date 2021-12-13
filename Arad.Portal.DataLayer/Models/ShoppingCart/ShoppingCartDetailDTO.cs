using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ShoppingCart
{
    public class ShoppingCartDetailDTO : ShoppingCartDetail
    {
        public ShoppingCartDetailDTO()
        {
            Notifications = new();
        }
        public int PreviousOrderCount { get; set; }

        /// <summary>
        /// finalPricePerUnit means discountPerUnit subtract From PricePerUnit
        /// </summary>
        public decimal PreviousFinalPricePerUnit { get; set; }

        public List<string> Notifications { get; set; }
    }
}
