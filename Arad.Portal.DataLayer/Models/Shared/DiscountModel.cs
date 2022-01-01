using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class DiscountModel
    {
        public long DiscountPerUnit { get; set; }

        public string PraisedProductId { get; set; }

        public int PraisedProductCount { get; set; }
    }
}
