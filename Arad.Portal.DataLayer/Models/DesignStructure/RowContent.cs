using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.DesignStructure
{
    public class RowContent
    {
        public RowContent()
        {
            ColsContent = new();
        }
        public int RowNumber { get; set; }

        public string ExtraClassNames { get; set; }

        public string InlineStyles { get; set; }

        public int? Order { get; set; }

        public List<ColContent> ColsContent { get; set; }


    }
}
