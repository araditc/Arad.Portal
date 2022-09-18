using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class ProductPageViewModel
    {
        public int CurrentPage { get; set; }
        public long ItemsCount { get; set; }
        public int PageSize { get; set; }
        public string Navigation { get; set; }
        public string QueryParams { get; set; }

        public SelectedFilter Filter { get; set; }
    }
}
