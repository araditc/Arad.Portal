using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class EntityCulture
    {
        public string LanguageId { get; set; }

        public string LanguageName { get; set; }

        public string CurrencyId { get; set; }

        public string CurrencyPrefix { get; set; }

        public string CurrencySymbol { get; set; }

        public string CurrencyName { get; set; }
    }
}
