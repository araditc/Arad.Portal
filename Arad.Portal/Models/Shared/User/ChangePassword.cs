using Arad.Portal.GeneralLibrary.CustomAttributes;

using System.ComponentModel.DataAnnotations;

namespace Arad.Portal.Models.Shared.User;

public class ChangePassword
{
    [ErrorMessage("AlertAndMessage_PasswordRequired")]
    public string Password { get; set; }

    [DataType(DataType.Password)]

    [ErrorMessage("AlertAndMessage_NewPasswordRequired")]
    public string NewPassword { get; set; }


    [ErrorMessage("AlertAndMessage_ReNewPasswordRequired")]
    public string ReNewPassword { get; set; }
}