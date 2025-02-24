using Arad.Portal.DataLayer.Models.Shared;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.ShoppingCart;

public class ShoppingCartDto
{
    public ShoppingCartDto()
    {
        Details = new();
    }
    public string Id { get; set; }

    public string OwnerId { get; set; }

    public string DomainId { get; set; }

    public string CouponCode { get; init; }

    public decimal? FinalPriceAfterCouponCode { get; init; }

    public decimal FinalPriceForPay { get; set; }

    public EntityCulture ShoppingCartCulture { get; set; }

    public List<PurchasePerSellerDto> Details { get; set; }
}