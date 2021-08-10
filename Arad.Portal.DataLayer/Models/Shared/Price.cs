using Arad.Portal.DataLayer.Entities.General.Currency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Arad.Portal.DataLayer.Models.Shared
{
    public class Price
    {
        public string PriceId { get; set; }

        public Entities.General.Currency.Currency Currency { get; set; }

        public decimal PriceValue { get; set; }

        public bool IsActive { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
