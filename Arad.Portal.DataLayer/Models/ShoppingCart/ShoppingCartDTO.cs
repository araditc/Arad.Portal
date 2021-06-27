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
            Details = new List<ShoppingCartDetail>();
        }
        public string UserCartId { get; set; }

        public EntityCulture ShoppingCartCulture { get; set; }

        public List<ShoppingCartDetail> Details { get; set; }
    }
}
