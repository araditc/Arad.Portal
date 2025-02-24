using Arad.Portal.DataLayer.Entities.Shop.Promotion;

using System;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Promotion;

public class PromotionDto
{
    public PromotionDto()
    {
        Infoes = new();
    }
    public string Id { get; init; }

    public string Title { get; init; }

    public PromotionType? PromotionType { get; init; }

    public int? PromotionTypeId { get; set; }

    public int DiscountTypeId { get; set; }

    public DiscountType DiscountType { get; init; }

    public long? Value { get; init; }

    public List<PromotionInfo> Infoes { get; init; }

    public bool AsUserCoupon { get; init; }

    public string CouponCode { get; init; }

    public string? ProductNamesConcat { get; set; }

    public string? PromotedProductId { get; init; }

    public string? PromotedProductName { get; init; }

    public string? GroupIdOfPromotedProduct { get; init; }

    public string? GroupNameofPromotedProduct { get; init; }

    public int? PromotedCountofUnit { get; init; }

    public int? BoughtCount { get; init; }

    public string CurrencyId { get; init; }

    public string CurrencyName { get; init; }

    public DateTime? SDate { get; init; }

    public string PersianStartDate { get; set; }

    public DateTime? EDate { get; init; }

    public string PersianEndDate { get; set; }

    public string? ModificationReason { get; init; }

    public bool IsDeleted { get; init; }

    public string AssociatedDomainId { get; set; }

}