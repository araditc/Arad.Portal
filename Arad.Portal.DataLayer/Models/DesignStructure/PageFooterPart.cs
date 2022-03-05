using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.DesignStructure
{
    public class PageFooterPart
    {
        public PageFooterPart()
        {
            CustomizedContent = new();
        }
        public string PriorFixedContent { get; set; }

        public List<RowContent> CustomizedContent { get; set; }

        public string LatterFixedContent { get; set; }
    }
}
