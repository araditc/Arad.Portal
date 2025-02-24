using Arad.Portal.DataLayer.Entities.Abstractions;
using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.Shop.Promotion;

public class UserCoupon: BaseEntity
{
    public List<string> UserIds { get; set; }

    public string PromotionId { get; set; }

    public string CouponCode { get; set; }

}