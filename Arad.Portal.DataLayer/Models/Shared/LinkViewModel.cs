using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class LinkViewModel
    {
        public string PerId { get; set; }
        public string MenuTitle { get; set; }
        public string Link { get; set; }
        public string Icon { get; set; }
        public bool IsActive { get; set; }
        public double Priority { get; set; }
        public List<LinkViewModel> Links { get; set; }
    }
}
