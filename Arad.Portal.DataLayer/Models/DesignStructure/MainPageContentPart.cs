using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.DesignStructure
{
    public class MainPageContentPart
    {
        public MainPageContentPart()
        {
            RowContents = new();
        }
        public List<RowContent> RowContents { get; set; }
    }
}
