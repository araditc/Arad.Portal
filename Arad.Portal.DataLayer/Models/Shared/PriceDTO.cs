using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class PriceDTO
    {
        public string PriceId { get; set; }

        public string CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        public string Symbol { get; set; }

        public string Prefix { get; set; }

        public long PriceValue { get; set; }

        public bool IsActive { get; set; }

        public string StartDate { get; set; }

        public DateTime? SDate { get; set; }

        public string EndDate { get; set; }

        public DateTime? EDate { get; set; }
    }
}
