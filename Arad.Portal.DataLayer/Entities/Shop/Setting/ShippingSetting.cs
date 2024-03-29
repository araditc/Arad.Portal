﻿
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.Setting
{
    /// <summary>
    /// if the site has shopping which buy any product to end user this entity explain detail of how product deliver to endUser
    /// </summary>
    public class ShippingSetting: BaseEntity
    {
        public ShippingSetting()
        {
            AllowedShippingTypes = new();
            ShippingCoupon = new();
        }
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string ShippingSettingId { get; set; }
        

        public List<ShippingTypeDetail> AllowedShippingTypes { get; set; }

        public ShippingCoupon ShippingCoupon { get; set; }

        public string CurrencyId { get; set; }

        public string CurrencySymbol { get; set; }

    }

    public class ShippingTypeDetail
    {
        /// <summary>
        /// the value property of basicData class with groupkey equal to  'ShippingType'
        /// </summary>
        public int ShippingTypeId { get; set; }

        /// <summary>
        /// the text property of BasicData class with groupkey equals to 'ShippingType'
        /// </summary>
        public string ShippingTypeText { get; set; }

        /// <summary>
        /// wether this shipping hasFixed expense or based on the weight and distance is dynamic expense
        /// </summary>
        public bool HasFixedExpense { get; set; }

        /// <summary>
        /// if HasFixedExpense = true then it is the fixed value of it
        /// </summary>
        public long FixedExpenseValue { get; set; }

        /// <summary>
        /// if it has any provider then it used that provider for calculating expense
        /// no provider has been implemented yet
        /// </summary>
        public string ProviderId { get; set; }

        public string ProviderName { get; set; }

    }

    public class ShippingCoupon
    {
        /// <summary>
        /// it is minimum price of invoice which need to use this shipping coupon
        /// </summary>
        public long FromInvoiceExpense { get; set; }
        /// <summary>
        /// if shipping expense equal zero it means shipping is free
        /// </summary>
        public long ShippingExpense { get; set; }

        /// <summary>
        ///  date when this shippingCoupon is valid
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartDate { get; set; }
        /// <summary>
        /// expire date of this shipping coupon
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EndDate { get; set; }
    }


}
