using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Arad.Portal.DataLayer.Entities.General.Currency
{
    public class Currency : BaseEntity
    {
        public string CurrencyId { get; set; }

        public string Name { get; set; }

        public string Prefix { get; set; }

        public string Symbol { get; set; }

        public bool IsDefault { get; set; }

    }
}
