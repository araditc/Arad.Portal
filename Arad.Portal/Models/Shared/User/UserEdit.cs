using System.ComponentModel.DataAnnotations;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using Arad.Portal.Models.Shared.Validator;

namespace Arad.Portal.Models.Shared.User;

public class UserEdit
{

    public string Id { get; init; }

    [CustomDisplayName("User_FirstName")]
    [Required(ErrorMessage = "AlertAndMessage_NameRequired")]
    public string FirstName { get; set; }

    [CustomDisplayName("User_LastName")]
    [Required(ErrorMessage = "AlertAndMessage_LastNameRequired")]
    public string LastName { get; set; }

    [MobilePhoneUserEditValidator]
    [CustomDisplayName("User_PhoneNumber")]
    [Required(ErrorMessage = "AlertAndMessage_PhoneNumberRequired")]
    public string PhoneNumber { get; set; }

    public bool IsVendor { get; init; }
    public bool IsSiteUser { get; init; }

    [DataType(DataType.EmailAddress, ErrorMessage = "AlertAndMessage_EmailInvalid")]
    public string Email { get; init; }

    [CustomDisplayName("User_FullMobile")]
    public string FullMobile { get; set; }

    [CustomDisplayName("User_Role")]
    [Required(ErrorMessage = "AlertAndMessage_UserRoleRequired")]
    public string UserRoleId { get; init; }

    [CustomDisplayName("DefaultLanguage")]
    [Required(ErrorMessage = "AlertAndMessage_DefaultLanguageRequired")]
    public string DefaultLanguageId { get; set; }

    [CustomDisplayName("DefaultLanguage")]
    public string? DefaultLanguageName { get; set; }

    [CustomDisplayName("DefaultCurrency")]
    public string? DefaultCurrencyId { get; init; }

    [CustomDisplayName("DefaultCurrency")]
    public string? DefaultCurrencyName { get; init; }
}