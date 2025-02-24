using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Product;

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