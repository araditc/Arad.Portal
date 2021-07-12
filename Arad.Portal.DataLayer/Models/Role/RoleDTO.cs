using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Role
{
    public class RoleDTO
    {
        public RoleDTO()
        {
            PermissionIds = new();
        }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsActive { get; set; }
        public List<string> PermissionIds { get; set; }
        public string ModificationReason { get; set; }
    }
}
