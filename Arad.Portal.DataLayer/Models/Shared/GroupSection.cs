using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class GroupSection
    {
        public GroupSection()
        {
            GroupsWithProducts = new List<string>();
        }
        public int CountToTake { get; set; }

        public int CountToSkip { get; set; }

        public string ProductGroupId { get; set; }

        public string DefaultLanguageId { get; set; }

        public long TotalCount { get; set; }

        public List<string> GroupsWithProducts { get; set; }
    }
}
