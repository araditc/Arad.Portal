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

        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string PromotionId { get; set; }

        public string Title { get; set; }

        public PromotionType PromotionType { get; set; }

        public DiscountType DiscountType { get; set; }

        public decimal? Value { get; set; }

        public string CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        public string AffectedProductId { get; set; }

        public string AffectedProductName { get; set; }

        public string AffectedProductGroupId { get; set; }
        public string AffectedProductGroupName { get; set; }

        public string PromotedProductId { get; set; }

        public string PromotedProductName { get; set; }

        public int? BoughtCount { get; set; }
        public int? PromotedCountofUnit { get; set; }


        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartDate { get; set; }


        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EndDate { get; set; }


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
