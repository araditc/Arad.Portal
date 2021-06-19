using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.Promotion
{
    public class Promotion
    {
        public string PromotionId { get; set; }
        

        public PromotionType PromotionType { get; set; }

        public DiscountType DiscountType { get; set; }

        public decimal Value { get; set; }

        public General.Currency.Currency Currency { get; set; }


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
        Percentage
    }
}
