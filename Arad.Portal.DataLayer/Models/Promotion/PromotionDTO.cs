using Arad.Portal.DataLayer.Entities.Shop.Promotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Promotion
{
    public class PromotionDTO
    {
        public string PromotionId { get; set; }

        public string Title { get; set; }

        public PromotionType PromotionType { get; set; }

        public DiscountType DiscountType { get; set; }

        public decimal? Value { get; set; }

        public string ProductId { get; set; }
        public string PromotedProductId { get; set; }

        public string PromotedProductName { get; set; }

        public int? PromotedCountofUnit { get; set; }

        public int? BoughtCount { get; set; }

        public string CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        public DateTime StartDate { get; set; }
       
        public DateTime? EndDate { get; set; }

        public string ModificationReason { get; set; }
        
    }
}
