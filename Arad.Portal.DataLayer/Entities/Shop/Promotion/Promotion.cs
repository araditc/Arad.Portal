using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.Promotion
{
    public class Promotion : BaseEntity
    {
        public Promotion()
        {
            Infoes = new List<PromotionInfo>();
        }
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string PromotionId { get; set; }

        public string Title { get; set; }
        /// <summary>
        /// what is the source of this promotion
        /// whether it is on productGroup or Product or all products
        /// if this record is asusercoupon then this enum is null
        /// </summary>
        public PromotionType? PromotionType { get; set; }

        /// <summary>
        /// if this field is set to true it means this record like a discount copon which admin can assign to any user of site
        /// </summary>
        public bool AsUserCoupon { get; set; }

        /// <summary>
        /// if this entity is UserCopoun then this field should have filled and be given to different users
        /// </summary>
        public string CouponCode { get; set; }

        /// <summary>
        /// what is the type of this discount whethere it is fixed or percentage of whole price or another product
        /// </summary>
        public DiscountType DiscountType { get; set; }

        /// <summary>
        /// if DiscountType = Fixed it is fixed amount of discount 
        /// if discountType = percentage it is the percentage value of discount for example 10 which means 10% of price
        /// and if discountType = product it is null or zero
        /// </summary>
        public long? Value { get; set; }

        /// <summary>
        /// currency of this promotion primary key of currency entity
        /// </summary>
        public string CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        /// <summary>
        /// if PromotionType = Group then for each object in this list AffectedProductGroupId and AffectedProductGroupName will be filled this list can be one or more
        /// if PromotionType = Product then for each object in this list AffectedProductId and AffectedProductName will be filled here 
        /// if PromotionType = All no filed require to fill here
        /// </summary>
        public List<PromotionInfo> Infoes { get; set; }

        /// <summary>
        /// if DiscountType = Product then this field is
        /// productId of which is as given as a gift in this promotion
        /// </summary>
        public string PromotedProductId { get; set; }


        /// <summary>
        /// if DiscountType = Product then this field is
        /// productName of which is given as a gift in this promotion
        /// </summary>
        public string PromotedProductName { get; set; }


        /// <summary>
        /// if DiscountType = Product then this field is
        /// productGroupId of product which is given as a gift in this promotion
        /// </summary>
        public string GroupIdOfPromotedProduct { get; set; }


        /// <summary>
        /// if DiscountType = Product then this field is
        /// productGroupName of product which is given as a gift in this promotion
        /// </summary>
        public string GroupNameofPromotedProduct { get; set; }

        /// <summary>
        /// determin how many units of promotedProduct will assign to this promotion as gift
        /// </summary>
        public int? PromotedCountofUnit { get; set; }

        /// <summary>
        /// determin how many units of product you should buy to use this this promotion for example if you buy "two" of this one of product in Infoes list then you can use this promotion
        /// </summary>
        public int? BoughtCount { get; set; }

        /// <summary>
        /// the startDate which this promotion occure
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime SDate { get; set; }


        /// <summary>
        /// expire date of this promotion
        /// </summary>

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EDate { get; set; }
    }

    public class PromotionInfo
    {
        public string AffectedProductId { get; set; }
        
        public string AffectedProductName { get; set; }

        public string AffectedProductGroupId { get; set; }

        public string AffectedProductGroupName { get; set; }

        public string ProductVendorId { get; set; }

        public string ProductVendorName { get; set; }
    }
    public enum PromotionType //if userCoupon = false then it has this field
    {
        All, //0
        Group, //1
        Product //2
    }

    public enum DiscountType
    {
        Fixed, //0
        Percentage, //1
        /// <summary>
        /// for example if you buy one you get another one free
        /// </summary>
        Product //2
    }
}
