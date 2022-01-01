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
        public PromotionDTO()
        {
            Infoes = new List<PromotionInfo>();
        }
        public string PromotionId { get; set; }

        public string Title { get; set; }

        public PromotionType PromotionType { get; set; }

        public int PromotionTypeId { get; set; }

        public int DiscountTypeId { get; set; }

        public DiscountType DiscountType { get; set; }

        public long? Value { get; set; }

        public List<PromotionInfo> Infoes { get; set; }

        public string ProductNamesConcat { get; set; }

        public string PromotedProductId { get; set; }

        public string PromotedProductName { get; set; }

        public string GroupIdOfPromotedProduct { get; set; }

        public string GroupNameofPromotedProduct { get; set; }

        public int? PromotedCountofUnit { get; set; }

        public int? BoughtCount { get; set; }

        public string CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        public DateTime? StartDate { get; set; }

        public string PersianStartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string PersianEndDate { get; set; }

        public string ModificationReason { get; set; }

        public bool IsDeleted { get; set; }

    }
}
