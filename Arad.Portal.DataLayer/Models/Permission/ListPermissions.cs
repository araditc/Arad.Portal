using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.DataLayer.Models.Permission
{
    public class ListPermissions
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public bool IsSelected { get; set; }
        public bool IsActive { get; set; }
        public List<ListPermissions> Childrens { get; set; }
        public PermissionType Type { get; set; }
        public double Priority { get; set; }
    }
}
