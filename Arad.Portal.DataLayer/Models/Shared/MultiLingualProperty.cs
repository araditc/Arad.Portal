using Arad.Portal.DataLayer.Entities.General.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Arad.Portal.DataLayer.Models.Shared
{
    public class MultiLingualProperty
    {
        public string MultiLingualPropertyId { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public string SeoTitle { get; set; }

        public string SeoDescription { get; set; }

        public string UrlFriend { get; set; }

        public string LanguageId { get; set; }

        public string LanguageName { get; set; }

        public string LanguageSymbol { get; set; }

        public string CurrencyId { get; set; }
       
        public string CurrencyPrefix { get; set; }

        public string CurrencySymbol { get; set; }

        public string CurrencyName { get; set; }
    }
}
