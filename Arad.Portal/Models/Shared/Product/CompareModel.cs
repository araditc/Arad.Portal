using Arad.Portal.DataLayer.Models.Shared;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Product;

public class CompareModel
{
    public CompareModel()
    {
        UnionSpecifications = new();
        ProductComapreList = new();
    }
    public List<SelectListModel> UnionSpecifications { get; init; }

    public List<ProductCompare> ProductComapreList { get; set; }

    public List<ProductCompare> SuggestionProducts { get; set; }
}