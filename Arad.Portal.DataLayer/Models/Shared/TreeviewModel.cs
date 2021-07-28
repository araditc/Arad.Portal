using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class TreeviewModel
    {
        public string id { get; set; }
        public string text { get; set; }
        public bool Checked { get; set; }

        public bool isActive { get; set; }
        public PermissionType type { get; set; }
        public double priority { get; set; }
        public List<TreeviewModel> children { get; set; }
    }
}
