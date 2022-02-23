using Arad.Portal.DataLayer.Models.DesignStructure;
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

        /// <summary>
        /// one of places of template for example: [x]
        /// </summary>
        public string Place { get; set; }
        public string ModuleId { get; set; }

        public ModuleParameters ParamsValue { get; set; }
    }
}
