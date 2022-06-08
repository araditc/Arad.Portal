using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.DesignStructure
{
    public class ColContent
    {
        public string Section { get; set; }
      
        public int? ColNumber { get; set; }
        /// <summary>
        /// classes are seperated by space as shown in class of each element tag
        /// </summary>
        public string ColumnClassNames { get; set; }

        public string InlineStyles { get; set; }

        public string ColData { get; set; }

        public string RowGuid { get; set; }
    }
}
