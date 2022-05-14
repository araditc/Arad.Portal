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

        public PromotionType PromotionType { get; set; }

        public DiscountType DiscountType { get; set; }

        public long? Value { get; set; }

        public string CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        /// <summary>
        /// this promotion is assigned to a product ,
        /// or list of products or a productGroup or list of productGroups
        /// </summary>
        public List<PromotionInfo> Infoes { get; set; }

        /// <summary>
        /// productId of which is as praise in this promotion
        /// </summary>
        public string PromotedProductId { get; set; }

        public string PromotedProductName { get; set; }

        public string GroupIdOfPromotedProduct { get; set; }    

        public string GroupNameofPromotedProduct { get; set; }

        /// <summary>
        /// how many units of promotedProduct with assign to this promotion as praise
        /// </summary>
        public int? PromotedCountofUnit { get; set; }

        public int? BoughtCount { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime SDate { get; set; }


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
    public enum PromotionType
    {
        All,
        Group, 
        Product
    }

    public enum DiscountType
    {
        Fixed,
        Percentage,
        /// <summary>
        /// for example if you buy one you get another free
        /// </summary>
        Product
    }
}
