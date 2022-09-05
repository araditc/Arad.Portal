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
        public LuceneSearchIndexModel()
        {
            GroupIds = new();
            TagKeywordList = new();
            GroupNames = new();
        }
        /// <summary>
        /// its whether product or content Id
        /// </summary>
        public string ID { get; set; }
        public string EntityName { get; set; }
        /// <summary>
        /// if this product  entity then it is productGroupId and it can be more than one
        /// if it is content entity then this is contentcategoryId and it is only one
        /// </summary>
        public List<string> GroupIds { get; set; }

        public string Code { get; set; }

        public string UniqueCode { get; set; }

        /// <summary>
        /// the productGroupName(single) or contentCategoryName(multiple) 
        /// </summary>
        public List<string> GroupNames { get; set; }

        /// <summary>
        /// list of tags in language
        /// </summary>
        public List<string> TagKeywordList { get; set; }


    }
}
