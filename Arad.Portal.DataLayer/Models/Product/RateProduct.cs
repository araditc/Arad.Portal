using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class RateProduct
    {
        public string ProductId { get; set; }
        public int Score { get; set; }
        public bool IsNew { get; set; }
    }
}
