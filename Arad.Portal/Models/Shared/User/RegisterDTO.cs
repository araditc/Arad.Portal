using Arad.Portal.GeneralLibrary.CustomAttributes;
using Arad.Portal.Models.Shared.Validator;
using System.ComponentModel.DataAnnotations;

namespace Arad.Portal.Models.Shared.User;

public class RegisterDto
{
    public string ReturnUrl { get; init; }

    [MobilePhoneUserEditValidator]
    [CustomDisplayName("User_PhoneNumber")]
    [Required(ErrorMessage = "AlertAndMessage_PhoneNumberRequired")]
    public string CellPhoneNumber { get; init; }

    public string FullCellPhoneNumber { get; set; }

    public string Email { get; init; }

    public string Username { get; init; }
    public string Captcha { get; init; }

    public string SecurityCode { get; init; }

    public string CurrentPass { get; init; }

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "AlertAndMessage_PasswordRequired")]
    [RegularExpression("^(?=.*[A-Za-z])(?=.*[0-9]).{4,}$", ErrorMessage = "AlertAndMessage_PasswordValidation")]
    [MinLength(6, ErrorMessage = "AlertAndMessage_MinLength")]
    public string NewPass { get; init; }

    [Required(ErrorMessage = "AlertAndMessage_RePasswordRequired")]
    [Compare(nameof(NewPass), ErrorMessage = "AlertAndMessage_PasswordRepassWordCompare")]
    public string ReNewPass { get; init; }
}