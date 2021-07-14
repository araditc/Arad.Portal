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
        public string typeTitle { get; set; }
        public string typeId { get; set; }
        public bool IsSelected { get; set; }
        public bool IsActive { get; set; }
        public List<ListPermissions> Menus { get; set; }
        public List<ListPermissions> Modules { get; set; }
        public PermissionType Type { get; set; }
    }
}
