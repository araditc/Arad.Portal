using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Arad.Portal.Models.Shared.Role;

public class RoleDto : IValidatableObject
{
    public string? Id { get; init; }
    public string Name { get; init; }
    public string PermissionIds { get; set; }
    public bool? IsActive { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new(GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_RequiredErrorMessage"), new[] { nameof(Name) });
        }

        if (string.IsNullOrEmpty(PermissionIds))
        {
            yield return new(GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_RequiredErrorMessage"), new[] { nameof(PermissionIds) });
        }
    }
}