using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.ContentCategory
{
    public class ContentCategory :BaseEntity
    {
        public ContentCategory()
        {
            CategoryNames = new();
        }
        public string ContentCategoryId { get; set; }

        public string ParentCategoryId { get; set; }
        /// <summary>
        /// name and languageId
        /// </summary>
        public List<MultiLingualProperty> CategoryNames { get; set; }

        public CategoryType CategoryType { get; set; }
    }

    public enum CategoryType
    {
        News = 0,
        Blog = 1,
        //etc...

    }
}
