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

        public BGType BGType { get; set; }
        public string BgImage { get; set; }
        public string CodeColor { get; set; }
        public string PriorFixedContent { get; set; }
        public List<RowContent> CustomizedContent { get; set; }
        public string LatterFixedContent { get; set; }
    } 
}
