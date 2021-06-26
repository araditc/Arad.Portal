using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Order
{
    public class OrderDetail
    {
        public string ProductId { get; set; }

        public string LanguageId { get; set; }

        public string LanguageName { get; set; }

        public string CurrencyId { get; set; }

        public int OrderCount { get; set; }

        public string ProductName { get; set; }

        public decimal PricePerUnit { get; set; }

        public decimal DiscountPerUnit { get; set; }

        public decimal ProductAmountToPay { get; set; }
    }
}
