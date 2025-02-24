using Arad.Portal.DataLayer.Models.Shared.Product;
using Arad.Portal.DataLayer.Models.Shared.ProductGroup;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.ProductGroup;

public class ProductGroupViewList
{
    public ProductGroupViewList()
    {
        Childs = new();
        Products = new();
    }
    public List<ProductGroupDto> Childs { get; set; }

    public List<ProductOutputDto> Products { get; set; }
}