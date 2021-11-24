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
        public MultiLingualProperty()
        {
            TagKeywords = new();
        }
        public string MultiLingualPropertyId { get; set; }

        public string Name { get; set; }

        public string GroupName { get; set; }

        public string Description { get; set; }
        /// <summary>
        /// this property comes from ckeditor
        /// </summary>
        public string BodyContent { get; set; }

        public string SeoTitle { get; set; }

        public string SeoDescription { get; set; }

        public string UrlFriend { get; set; }

        ///// <summary>
        ///// all possible values which this Name has
        ///// </summary>
        public List<string> NameValues { get; set; }

        public string LanguageId { get; set; }

        public string LanguageName { get; set; }

        public string LanguageSymbol { get; set; }

        public string CurrencyId { get; set; }
       
        public string CurrencyPrefix { get; set; }

        public string CurrencySymbol { get; set; }

        public string CurrencyName { get; set; }

        public string Tag { get; set; }

        public List<string> TagKeywords { get; set; }

    }
}
