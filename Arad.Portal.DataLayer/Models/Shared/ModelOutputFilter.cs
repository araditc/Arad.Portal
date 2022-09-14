using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class ModelOutputFilter
    {
        public ModelOutputFilter()
        {
            Filters = new();
        }
        public decimal MinPrice { get; set; }

        public decimal MaxPrice { get; set; }
        
        public int Step { get; set; }

        public List<DynamicFilter> Filters { get; set; }
    }
}
