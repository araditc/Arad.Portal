using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.General.Role
{
    public class Role : BaseEntity
    {
        public Role()
        {
            PermissionIds = new();
        }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<string> PermissionIds { get; set; }

    }
}
