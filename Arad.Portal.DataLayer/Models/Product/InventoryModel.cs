using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Product
{
    public class InventoryModel
    {
        public InventoryModel()
        {
            SelectedSpecs = new();
        }
        public string ProductId { get; set; }
        public int? Count { get; set; }
        public List<SelectListModel> SelectedSpecs { get; set; }
    }

   
}
