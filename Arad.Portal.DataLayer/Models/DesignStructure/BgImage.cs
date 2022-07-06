using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.DesignStructure
{
    public class BgImage
    {
        public string Base64ImageContent { get; set; }

        public string SelectedRowGuid { get; set; }

        public string SelectedSection { get; set; }
    }
}
