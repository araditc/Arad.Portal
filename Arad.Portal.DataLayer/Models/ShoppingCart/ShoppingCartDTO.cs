using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
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

        public List<ShoppingCartDetail> Details { get; set; }
    }
}
