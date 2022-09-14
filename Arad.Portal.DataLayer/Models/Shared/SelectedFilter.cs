using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class SelectedFilter
    {
        public SelectedFilter()
        {
            SelectedDynamicFilters = new();
            GroupIds = new();
        }
        public decimal FirstPrice { get; set; }

        public decimal LastPrice { get; set; }

        public List<string> GroupIds { get; set; }

        public bool IsAvailable { get; set; }

        public List<SelectedDynamicFilter> SelectedDynamicFilters { get; set; }
    }
}
