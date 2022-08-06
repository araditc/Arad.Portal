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
            ProductGroupNames = new();
        }
        public string MultiLingualPropertyId { get; set; }

        public string Name { get; set; }

        public string GroupName { get; set; }

        public List<string> ProductGroupNames { get; set; }

        public string Description { get; set; }
        /// <summary>
        /// this property comes from ckeditor
        /// </summary>
        public string BodyContent { get; set; }

        /// <summary>
        /// the title which is used in serach engin optimization
        /// </summary>
        public string SeoTitle { get; set; }


        /// <summary>
        /// the description which is used in serach engin optimization
        /// </summary>
        public string SeoDescription { get; set; }

        /// <summary>
        /// the customized url to access entity it is based on language
        /// </summary>
        public string UrlFriend { get; set; }

        /// <summary>
        /// the help for purchasing each product (not implemented)
        /// </summary>
        public string PurchaseHelp { get; set; }

        ///// <summary>
        ///// all possible values which this Name has
        /// this field is using in specification
        ///// </summary>
        public List<string> NameValues { get; set; }

        public string LanguageId { get; set; }

        public string LanguageName { get; set; }

        public string LanguageSymbol { get; set; }

        public string CurrencyId { get; set; }
       
        public string CurrencyPrefix { get; set; }

        public string CurrencySymbol { get; set; }

        public string CurrencyName { get; set; }

        /// <summary>
        /// it store the rowNumber of this object in ProductEntity for insertion and edition
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// tag keywords which can be used in searching 
        /// </summary>
        public List<string> TagKeywords { get; set; }

    }
}
