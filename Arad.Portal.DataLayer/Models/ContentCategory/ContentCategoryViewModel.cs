using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ContentCategory
{
    public class ContentCategoryViewModel
    {
        public string ContentCategoryId { get; set; }

        public string ParentCategoryId { get; set; }
        /// <summary>
        /// name and languageId
        /// </summary>
        public MultiLingualProperty CategoryName { get; set; }

        public CategoryType CategoryType { get; set; }

        public bool IsDeleted { get; set; }
    }
}

