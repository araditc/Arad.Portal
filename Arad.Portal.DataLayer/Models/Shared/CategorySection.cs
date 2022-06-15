using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class CategorySection
    {
        public CategorySection()
        {
            CategoriesWithContent = new List<string>();
        }
        public int CountToTake { get; set; }

        public int CountToSkip { get; set; }

        public string ContentCategoryId { get; set; }

        public string DefaultLanguageId { get; set; }

        public long TotalCount { get; set; }

        public List<string> CategoriesWithContent { get; set; }
    }
}
