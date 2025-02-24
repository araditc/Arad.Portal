using Arad.Portal.GeneralLibrary.CustomAttributes;

namespace Arad.Portal.Models.Shared.User;

public class UserChangePasswordModel
{
    public string UserId { get; set; }

    [ErrorMessage("AlertAndMessages_InputRequiredErrorMessage")]
    public string OldPass { get; set; }

    [ErrorMessage("AlertAndMessages_InputRequiredErrorMessage")]
    public string NewPass { get; set; }

    [ErrorMessage("AlertAndMessages_InputRequiredErrorMessage")]
    public string RepNewPass { get; set; }
}