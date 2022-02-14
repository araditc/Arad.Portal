using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class ModuleParams
    {
        public ModuleParams()
        {
            ParamsValue = new();
        }
        public string ModuleId { get; set; }

        public List<KeyVal> ParamsValue { get; set; }
    }
}
