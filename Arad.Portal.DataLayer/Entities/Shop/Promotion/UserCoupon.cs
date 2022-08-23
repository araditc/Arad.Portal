using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.Shop.Promotion
{
    public class UserCoupon: BaseEntity
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public string UserCouponId { get; set; }

        public string UserId { get; set; }

        public string PromotionId { get; set; }

        public string CouponCode { get; set; }

    }
}
