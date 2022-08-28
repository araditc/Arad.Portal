using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ShoppingCart
{
    public class ShoppingCartDTO
    {
        public ShoppingCartDTO()
        {
            Details = new List<PurchasePerSellerDTO>();
        }
        public string ShoppingCartId { get; set; }

        public string OwnerId { get; set; }

        public string DomainId { get; set; }

        public string CouponCode { get; set; }

        public long? FinalPriceAfterCouponCode { get; set; }

        public long FinalPriceForPay { get; set; }

        public EntityCulture ShoppingCartCulture { get; set; }

        public List<PurchasePerSellerDTO> Details { get; set; }
    }
}
