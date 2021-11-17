using Arad.Portal.DataLayer.Models.Content;
using Arad.Portal.DataLayer.Models.ContentCategory;
using Arad.Portal.DataLayer.Models.Product;
using Arad.Portal.DataLayer.Models.ProductGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class CommonViewModel
    {
        public List<ProductGroupDTO> Groups { get; set; }
        public List<ProductOutputDTO> ProductList { get; set; }
        public List<ContentCategoryDTO> Categories { get; set; }
        public List<ContentViewModel> BlogList { get; set; }
        public ProductOutputDTO ProductDetail { get; set; }
        public ContentDTO ContentDetail { get; set; }
        public GroupSection GroupSection { get; set; }
        public ProductsInGroupSection ProductInGroupSection { get; set; }
    }
}
