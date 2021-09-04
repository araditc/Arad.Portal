using Arad.Portal.DataLayer.Entities.General.ContentCategory;
using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.ContentCategory
{
    public class ContentCategoryDTO
    {
        public ContentCategoryDTO()
        {
            CategoryNames = new();
        }
        public string ContentCategoryId { get; set; }

        public string ParentCategoryId { get; set; }
        /// <summary>
        /// name and languageId
        /// </summary>
        public List<MultiLingualProperty> CategoryNames { get; set; }

        public ContentCategoryType CategoryType { get; set; }

        public int CategoryTypeId { get; set; }

        public string ModificationReason { get; set; }

        public bool IsDeleted { get; set; }
    }
}
