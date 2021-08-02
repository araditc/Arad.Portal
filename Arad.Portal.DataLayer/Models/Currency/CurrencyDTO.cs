using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Currency
{
    public class CurrencyDTO
    {
        public string CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        public string Prefix { get; set; }

        public string Symbol { get; set; }

        public bool IsDefault { get; set; }

        public string ModificationReason { get; set; }

    }
}
