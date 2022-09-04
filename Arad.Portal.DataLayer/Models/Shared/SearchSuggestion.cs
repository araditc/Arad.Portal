using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class SearchSuggestion
    {
        public string InitialKeyword { get; set; }

        public List<string> ContentCategoryIds { get; set; }

        public List<string> ProductGroupIds { get; set; }
    }
}
