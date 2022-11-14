using Arad.Portal.GeneralLibrary.CustomAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Role
{
    public class UserRoleViewModel
    {
        public string Id { get; set; }

        public List<string> PermissionIds { get; set; } = new List<string>();
        public bool IsEditView { get; set; }

        [ErrorMessage("AlertAndMessage_FieldEssential")]
        public string RoleName { get; set; }
        public List<string> SelectedPermissions { get; set; }
        //public List<PerSelect> AllAllowedPermissions { get; set; }
        
        public string ModificationReason { get; set; }
        public string Color { get; set; }
    }
}
