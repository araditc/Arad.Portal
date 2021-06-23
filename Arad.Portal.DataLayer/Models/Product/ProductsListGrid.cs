using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class ProductsListGrid
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public List<string> GroupNames { get; set; }
        public List<string> GroupIds { get; set; }
        public string UniqueCode { get; set; }
        public string Inventory { get; set; }
    }
}
