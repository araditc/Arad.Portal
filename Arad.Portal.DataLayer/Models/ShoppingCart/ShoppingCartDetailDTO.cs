using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Models.Shared;
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

        public int RowNumber { get; set; }
        public int PreviousOrderCount { get; set; }
        /// <summary>
        /// finalPricePerUnit means discountPerUnit subtract From PricePerUnit
        /// </summary>
        public long PreviousFinalPricePerUnit { get; set; }

        public string ShoppingCartDetailId { get; set; }

        public Image ProductImage { get; set; }

        public List<string> Notifications { get; set; }
    }
}
