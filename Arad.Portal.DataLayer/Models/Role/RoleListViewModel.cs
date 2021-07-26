using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Role
{
    public class RoleListViewModel
    {
        public string Id { get; set; }
        public string RoleName { get; set; }
        public string CreatorId { get; set; }
        public string CreatorUserName { get; set; }
        public DateTime CreationDateTime { get; set; }
        public bool HasModifications { get; set; }
    }
}
