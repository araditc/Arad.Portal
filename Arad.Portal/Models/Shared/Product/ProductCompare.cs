using Arad.Portal.DataLayer.Models.Shared.Product;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Product;

public class ProductCompare
{
    public ProductCompare()
    {
        Specifications = new();
    }
    public string ProductId { get; init; }

    public string ProductName { get; init; }

    public long ProductCode { get; init; }

    public decimal CurrentPrice { get; init; }

    public string FormattedPrice { get; set; }

    public string ProductImageUrl { get; set; }

    public List<ProductSpecificationValue> Specifications { get; init; }
}