using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Models.Product;
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
            ProductSpecValues = new();
        }
        public string ShoppingCartDetailId { get; set; }

        public string ProductId { get; set; }
        public int RowNumber { get; set; }
        public int PreviousOrderCount { get; set; }
        /// <summary>
        /// finalPricePerUnit means discountPerUnit subtract From PricePerUnit
        /// </summary>
        public decimal PreviousFinalPricePerUnit { get; set; }

        public Image ProductImage { get; set; }

        public long ProductCode { get; set; }

        public List<SpecValue> ProductSpecValues { get; set; }

        public List<string> Notifications { get; set; }
    }
}
