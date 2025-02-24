using Arad.Portal.DataLayer.Entities.Shop.ProductSpecificationGroup;
using Arad.Portal.DataLayer.Models.Shared.Product;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Product;

public class ProductSpecViewModel
{
    public ProductSpecViewModel()
    {
        ProductSpecificationValues = new();
    }
    public ProductSpecGroup SpecGroup { get; set; }
    public List<ProductSpecificationValue> ProductSpecificationValues { get; set; }
}