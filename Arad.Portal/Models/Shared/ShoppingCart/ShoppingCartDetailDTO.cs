using Arad.Portal.DataLayer.Entities.Shop.ShoppingCart;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Shared.Product;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.ShoppingCart;

public class ShoppingCartDetailDto : ShoppingCartDetail
{
    public ShoppingCartDetailDto()
    {
        Notifications = [];
        ProductSpecValues = [];
    }
    public string Id { get; init; }

    public string ProductId { get; init; }
    public int RowNumber { get; init; }
    public int PreviousOrderCount { get; init; }
    /// <summary>
    /// finalPricePerUnit means discountPerUnit subtract From PricePerUnit
    /// </summary>
    public decimal PreviousFinalPricePerUnit { get; set; }

    public Image ProductImage { get; set; }

    public long ProductCode { get; init; }

    public List<SpecValue> ProductSpecValues { get; init; }

    public List<string> Notifications { get; init; }
}