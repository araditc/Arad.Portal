using Arad.Portal.GeneralLibrary.CustomAttributes;

namespace Arad.Portal.Models.Shared.Permission;

public class UserPermissionViewModel
{
    public string ModificationReason { get; init; }
    public bool IsEditView { get; init; }

    public double Priority { get; init; }
    public string Id { get; init; }

    //[CustomErrorMessage("AlertAndMessage_FieldEssential")]
    //public Enums.PermissionType Type { get; set; }
    //[CustomErrorMessage("AlertAndMessage_FieldEssential")]
    //public Enums.PermissionMethod Method { get; set; }

    [ErrorMessage("AlertAndMessage_FieldEssential")]
    public string Title { get; init; }

    public string MenuIdOfModule { get; init; }

    public string ParentMenuId { get; init; }
    public string Routes { get; init; }
    public string Icon { get; init; }
    public string ClientAddress { get; init; }

}