using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class CompareModel
    {
        public CompareModel()
        {
            UnionSpecifications = new();
            ProductComapreList = new();
        }
        public List<SelectListModel> UnionSpecifications { get; set; }

        public List<ProductCompare> ProductComapreList { get; set; }

        public List<ProductCompare> SuggestionProducts { get; set; }
    }
}
