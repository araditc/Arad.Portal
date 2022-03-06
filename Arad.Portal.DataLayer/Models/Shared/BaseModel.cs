using Arad.Portal.DataLayer.Models.DesignStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public abstract class BaseModel
    {
        public PageHeaderPart HeaderPart { get; set; }

        public PageFooterPart FooterPart { get; set; }
    }
}
