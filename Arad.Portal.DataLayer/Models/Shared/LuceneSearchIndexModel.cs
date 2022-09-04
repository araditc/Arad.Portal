using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    /// <summary>
    /// implemented one index per language
    /// </summary>
    public class LuceneSearchIndexModel
    {
        /// <summary>
        /// if it is product it start with pro_ProductId and if it is content it start with con_ContentId
        /// </summary>
        public string EntityId { get; set; }

        
        public string EntityName { get; set; }

        /// <summary>
        /// if this product  entity then it is productGroupId 
        /// if it is content entity then this is contentcategoryId
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// the productGroupName or contentCategoryName 
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// list of tags in all languages
        /// </summary>
        public string TagKeywordList { get; set; }


    }
}
