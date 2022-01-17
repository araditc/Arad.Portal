using Arad.Portal.DataLayer.Entities.Shop.Setting;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Setting
{
    public class ShippingSettingDTO
    {
        public ShippingSettingDTO()
        {
            AllowedShippingTypes = new();
            ShippingCoupon = new();
        }

        public string ShippingSettingId { get; set; }

        public string AssociatedDomainId { get; set; }

        public string DomainName { get; set; }

        public string CurrencyId { get; set; }

        public string CurrencySymbol { get; set; }

        public List<ShippingTypeDetailDTO> AllowedShippingTypes { get; set; }

        public ShippingCouponDTO ShippingCoupon { get; set; }
    }

    public class ShippingTypeDetailDTO
    {
        /// <summary>
        /// the value property of basicData class with groupkey equal to  'ShippingType'
        /// </summary>
        public int ShippingTypeId { get; set; }

        /// <summary>
        /// the text property of BasicData class with groupkey equals to 'ShippingType'
        /// </summary>
        public string ShippingTypeText { get; set; }

        public bool HasFixedExpense { get; set; }

        public decimal FixedExpenseValue { get; set; }

        public string ProviderId { get; set; }

        public string ProviderName { get; set; }

        public decimal FloatingExpense { get; set; }

    }


    public class ShippingCouponDTO
    {
        public ShippingCouponDTO()
        {
            FromInvoiceExpense = 0;
            ShippingExpense = 0;

        }
        //public string ShippingCouponId { get; set; }
        public decimal FromInvoiceExpense { get; set; }

        /// <summary>
        /// if shipping expense equal zero it means shipping is free
        /// </summary>
        public decimal ShippingExpense { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? StartDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EndDate { get; set; }

        public string PersianStartDate { get; set; }

        public string PersianEndDate { get; set; }
    }

}
