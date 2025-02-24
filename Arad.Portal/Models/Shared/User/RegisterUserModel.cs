using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Arad.Portal.DataLayer.Models.Shared.User;
using Arad.Portal.Models.Shared.Role;
using Arad.Portal.Models.Shared.Validator;

namespace Arad.Portal.Models.Shared.User;

public class RegisterUserModel
{
    // Username is required and must have a minimum length of 3 characters.
    [Required(ErrorMessage = "AlertAndMessage_UserNameRequired")]
    [MinLength(3, ErrorMessage = "AlertAndMessage_MinLength")]
    public string UserName { get; init; }

    // Name is required.
    [Required(ErrorMessage = "AlertAndMessage_NameRequired")]
    public string Name { get; init; }

    // LastName is required.
    [Required(ErrorMessage = "AlertAndMessage_LastNameRequired")]
    public string LastName { get; init; }

    // Optional field for the Father's name.
    public string? FatherName { get; init; }

    // Gender is a required enum field.
    public Gender Gender { get; init; }

    // Custom mobile phone validation.
    [MobilePhoneValidator]
    public string PhoneNumber { get; init; }

    // Full mobile is optional but can be used by your validator.
    public string FullMobile { get; init; }

    // Default language is required.
    [Required(ErrorMessage = "AlertAndMessage_DefaultLanguageRequired")]
    public string DefaultLanguageId { get; init; }

    // Display name of the language (Optional).
    public string? DefaultLanguageName { get; init; }

    // Email must be a valid email format.
    [DataType(DataType.EmailAddress, ErrorMessage = "AlertAndMessage_EmailInvalid")]
    public string Email { get; init; }

    // Default currency ID and name, populated later.
    public string? DefaultCurrencyId { get; set; }
    public string? DefaultCurrencyName { get; set; }

    // IsActive is true by default.
    public bool IsActive { get; init; } = true;

    // User status flags.
    public bool IsVendor { get; init; }
    public bool IsSiteUser { get; init; }

    // Role ID is required.
    [Required(ErrorMessage = "AlertAndMessage_UserRoleRequired")]
    public string UserRoleId { get; init; }

    // Password must meet specific validation criteria (length, alphanumeric).
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "AlertAndMessage_PasswordRequired")]
    [RegularExpression("^(?=.*[A-Za-z])(?=.*[0-9]).{4,}$", ErrorMessage = "AlertAndMessage_PasswordValidation")]
    [MinLength(6, ErrorMessage = "AlertAndMessage_MinLength")]
    public string Password { get; init; }

    // Re-enter password, must match the original password.
    [Required(ErrorMessage = "AlertAndMessage_RePasswordRequired")]
    [Compare(nameof(Password), ErrorMessage = "AlertAndMessage_PasswordRepassWordCompare")]
    public string RePassword { get; init; }

    // List of roles.
    public List<RoleListView> Roles { get; set; } = [];
}
