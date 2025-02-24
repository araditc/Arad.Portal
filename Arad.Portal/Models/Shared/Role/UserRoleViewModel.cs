using Arad.Portal.GeneralLibrary.CustomAttributes;

using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.Role;

public class UserRoleViewModel
{
    public string Id { get; set; }

    public List<string> PermissionIds { get; set; } = new();
    public bool IsEditView { get; set; }

    [ErrorMessage("AlertAndMessage_FieldEssential")]
    public string RoleName { get; set; }
    public List<string> SelectedPermissions { get; set; }
    //public List<PerSelect> AllAllowedPermissions { get; set; }

    public string ModificationReason { get; set; }
    public string Color { get; set; }
}