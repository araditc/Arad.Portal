using Arad.Portal.GeneralLibrary.CustomAttributes;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Arad.Portal.Models.Shared.Permission;

public class PermissionDto : IValidatableObject
{
    public string ParentId { get; set; }

    public string Title { get; set; }

    public string ClientAddress { get; set; }

    public string Routes { get; set; }

    [CustomInteger]
    public int Priority { get; set; }

    public string Icon { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Priority <= 0)
        {
            yield return new(GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_NumberLessThanZero"), new[] { nameof(Priority) });
        }

        if (string.IsNullOrWhiteSpace(Icon))
        {
            yield return new(GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_RequiredErrorMessage"), new[] { nameof(Icon) });
        }

        if (string.IsNullOrWhiteSpace(ClientAddress))
        {
            yield return new(GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_RequiredErrorMessage"), new[] { nameof(ClientAddress) });
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            yield return new(GeneralLibrary.Utilities.UtilityLanguage.GetString("AlertAndMessage_RequiredErrorMessage"), new[] { nameof(Title) });
        }
    }
}

public class PermissionTreeViewDto
{
    public string Id { get; init; }

    public string Title { get; init; }

    public short LevelNo { get; init; }

    public short Priority { get; init; }

    public string Icon { get; init; }

    public string ClientAddress { get; set; }

    public bool Checked { get; init; }

    public bool IsActive { get; set; }

    public List<string> Urls { get; init; }

    public List<PermissionTreeViewDto> Children { get; init; } = new();

    public List<ActionDto> Actions { get; init; } = new();
}

public class PermissionSelectDto
{
    public string PermissionId { get; set; }

    public string CreatorUserId { get; set; }

    public string CreatorUserName { get; set; }

    public string CreationDate { get; set; }

    public bool IsActive { get; set; }


    public bool HasModification { get; set; }
    public string Title { get; set; }

    public string ParentTitle { get; set; }

    public short LevelNo { get; set; }

    public short Priority { get; set; }

    public string Icon { get; set; }

    public string ClientAddress { get; set; }
}

public class ActionDto
{
    public string Id { get; init; }

    public string Title { get; init; }

    public string ClientAddress { get; init; }

    public List<string> Urls { get; init; }
}