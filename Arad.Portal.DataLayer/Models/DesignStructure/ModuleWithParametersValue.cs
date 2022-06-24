using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.DesignStructure
{
    public class ModuleWithParametersValue
    {
        public ModuleWithParametersValue()
        {
            ParametersValue = new ModuleParameters();
        }
        public string ModuleId { get; set; }

        public string ModuleName { get; set; }

        public ModuleParameters ParametersValue { get; set; }
    }
}
