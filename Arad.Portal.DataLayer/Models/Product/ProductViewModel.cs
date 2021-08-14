using Arad.Portal.DataLayer.Entities.Shop.ProductUnit;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductViewModel
    {
        public string ProductId { get; set; }
        public List<string> GroupNames { get; set; }
        public List<string> GroupIds { get; set; }
        public string UniqueCode { get; set; }
        public int Inventory { get; set; }
        public MultiLingualProperty MultiLingualProperty { get; set; }
        public string MainImage { get; set; }
        public Price Price { get; set; }
        public ProductUnitViewModel Unit { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
