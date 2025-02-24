using System;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.User;

public class UserCouponDto
{
    public string Id { get; set; }

    public List<string> UserIds { get; set; }

    public List<string> UserNames { get; init; }

    public string PromotionId { get; set; }

    public string PromotionName { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DataLayer.Entities.Shop.Promotion.DiscountType? DiscountType { get; set; }

    public long? Value { get; set; }

    public string CouponCode { get; set; }

    public string AssociatedDomainId { get; set; }

    public bool IsDeleted { get; init; }
}