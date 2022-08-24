using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class UserCouponDTO
    {
        public string UserCouponId { get; set; }

        public List<string> UserIds { get; set; }

       public List<string> UserNames { get; set; }

        public string PromotionId { get; set; }

        public string PromotionName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public Entities.Shop.Promotion.DiscountType? DiscountType { get; set; }

        public long? Value { get; set; }

        public string CouponCode { get; set; }

        public string AssociatedDomainId { get; set; }

        public bool  IsDeleted { get; set; }
    }
}
