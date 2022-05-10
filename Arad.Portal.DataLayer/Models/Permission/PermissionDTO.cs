using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Permission
{
    //public class PermissionDTO
    //{
    //    public string PermissionId { get; set; }
    //    public string Title { get; set; }
    //    public string Routes { get; set; }
    //    //public string Route { get; set; }
    //    public string ClientAddress { get; set; }
    //    public Enums.PermissionType Type { get; set; }
    //    public Enums.PermissionMethod Method { get; set; }

    //    public string MenuIdOfModule { get; set; }

    //    /// <summary>
    //    /// ParentMenu
    //    /// </summary>
    //    public string ParentMenuId { get; set; }
    //    public double Priority { get; set; }
    //    public string Icon { get; set; }
    //    public bool IsEditView { get; set; }
    //    public string ModificationReason { get; set; }
    //}

    public class PermissionDto :  IValidatableObject
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
                yield return new(GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_NumberLessThanZero"), new[] { nameof(Priority) });
            }

            if (string.IsNullOrWhiteSpace(Icon))
            {
                yield return new(GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_RequiredErrorMessage"), new[] { nameof(Icon) });
            }

            if (string.IsNullOrWhiteSpace(ClientAddress))
            {
                yield return new(GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_RequiredErrorMessage"), new[] { nameof(ClientAddress) });
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                yield return new(GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_RequiredErrorMessage"), new[] { nameof(Title) });
            }
        }
    }

    public class PermissionTreeViewDto
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public short LevelNo { get; set; }

        public short Priority { get; set; }

        public string Icon { get; set; }

        public string ClientAddress { get; set; }

        public bool Checked { get; set; }

        public bool IsActive { get; set; }

        public List<string> Urls { get; set; }

        public List<PermissionTreeViewDto> Children { get; set; } = new();

        public List<ActionDto> Actions { get; set; } = new();
    }

    public class PermissionSelectDto 
    {
        public string PermissionId  { get; set; }

        public string CreatorUserId { get; set; }

        public string CreatorUserName { get; set; }

        public string CreationDate { get; set; }

        public bool IsActive { get; set; }

        public string Title { get; set; }

        public string ParentTitle { get; set; }

        public short LevelNo { get; set; }

        public short Priority { get; set; }

        public string Icon { get; set; }

        public string ClientAddress { get; set; }
    }

    public class ActionDto
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string ClientAddress { get; set; }

        public List<string> Urls { get; set; }
    }

}
