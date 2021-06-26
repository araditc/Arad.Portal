using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductsListGrid
    {
        public ProductsListGrid()
        {
            MultiLingualProperties = new();
        }
        public string ProductId { get; set; }
        public List<string> GroupNames { get; set; }
        public List<string> GroupIds { get; set; }
        public string UniqueCode { get; set; }
        public int Inventory { get; set; }
        public List<MultiLingualProperty> MultiLingualProperties { get; set; }
    }
}
