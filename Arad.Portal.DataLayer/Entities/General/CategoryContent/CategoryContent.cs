using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.CategoryContent
{
    public class CategoryContent
    {
        public CategoryContent()
        {
            CategoryNames = new();
        }
        public string CategoryContentId { get; set; }

        public string ParentCategoryId { get; set; }

        public List<MultiLingualProperty> CategoryNames { get; set; }
    }
}
