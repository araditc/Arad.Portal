using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Setting;

public class ShippingSettingDto
{
    public ShippingSettingDto()
    {
        AllowedShippingTypes = new();
        ShippingCoupon = new();
    }

    public string Id { get; set; }

    public string AssociatedDomainId { get; init; }

    public string DomainName { get; set; }

    public string CurrencyId { get; init; }

    public string CurrencySymbol { get; set; }

    public bool IsDeleted { get; init; }

    public List<ShippingTypeDetailDto> AllowedShippingTypes { get; init; }

    public ShippingCouponDto ShippingCoupon { get; init; }
}

public class ShippingTypeDetailDto
{
    /// <summary>
    /// The value property of BasicData class with group key equal to 'ShippingType'
    /// </summary>
    public int ShippingTypeId { get; init; }

    /// <summary>
    /// The text property of BasicData class with group key equal to 'ShippingType'
    /// </summary>
    public string ShippingTypeText { get; init; }

    public bool HasFixedExpense { get; init; } = false;

    public int? FixedExpenseValue { get; init; }

    public string ProviderId { get; init; }

    public string ProviderName { get; init; }

    public long FloatingExpense { get; init; }
}

public class ShippingCouponDto
{
    public ShippingCouponDto()
    {
        FromInvoiceExpense = 0;
        ShippingExpense = 0;
    }

    //public string ShippingCouponId { get; set; }
    public long FromInvoiceExpense { get; init; }

    /// <summary>
    /// If shipping expense equals zero, it means shipping is free
    /// </summary>
    public long ShippingExpense { get; init; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? StartDate { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? EndDate { get; set; }

    public string PersianStartDate { get; set; }

    public string PersianEndDate { get; set; }
}