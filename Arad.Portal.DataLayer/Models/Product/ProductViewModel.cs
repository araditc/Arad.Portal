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
        public ProductViewModel()
        {
            MultiLingualProperties = new List<MultiLingualProperty>();
            Images = new List<Image>();
            Prices = new List<Price>();
        }
        public string ProductId { get; set; }
        public List<string> GroupNames { get; set; }
        public List<string> GroupIds { get; set; }
        public string UniqueCode { get; set; }
        public int Inventory { get; set; }
        public List<MultiLingualProperty> MultiLingualProperties { get; set; }
        public List<Image> Images { get; set; }
        public List<Price> Prices { get; set; }
        public ProductUnit Unit { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
