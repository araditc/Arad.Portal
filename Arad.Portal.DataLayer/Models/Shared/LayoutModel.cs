using Arad.Portal.DataLayer.Models.DesignStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public  class LayoutModel
    {
        public bool HasCustomizedHeader { get; set; }
        public PageHeaderPart HeaderPart { get; set; }
        public bool  HasCustomizedFooter { get; set; }
        public PageFooterPart FooterPart { get; set; }
        public bool IsMultiLingual { get; set; }
        public bool IsShop { get; set; }
    }
}
