using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class BasketModel
    {
        public BasketModel()
        {
            SpecVals = new();
        }
        public string Code { get; set; }

        public int Count { get; set; }

        public string  CartDetailId { get; set; }

        public List<SpecValue> SpecVals { get; set; }
    }
}
