using System.Collections.Generic;

namespace Arad.Portal.DataLayer.Entities.General.Role
{
    public class Role : BaseEntity
    {
        public Role()
        {
            Permissions = new();
        }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<Permission.Permission> Permissions { get; set; }

    }
}
