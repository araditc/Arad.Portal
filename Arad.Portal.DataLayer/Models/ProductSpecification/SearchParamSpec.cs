using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ProductSpecification
{
    public class SearchParamSpec
    {
        public string Title { get; set; }
        public string value { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
