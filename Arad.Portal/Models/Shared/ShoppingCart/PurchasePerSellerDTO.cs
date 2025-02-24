using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.ShoppingCart;

public class PurchasePerSellerDto
{
    public PurchasePerSellerDto()
    {
        Products = new();
    }
    public string SellerId { get; init; }

    public string SellerUserName { get; set; }

    public List<ShoppingCartDetailDto> Products { get; init; }

    public int ShippingTypeId { get; set; }

    public decimal ShippingExpense { get; set; }

    public decimal TotalDetailsAmountWithShipping { get; set; }
}