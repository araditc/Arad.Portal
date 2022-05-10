using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.Permission;

namespace Arad.Portal.DataLayer.Models.Role
{
    public class RoleDTO : IValidatableObject
    {
        public string RoleId { get; set; }        
        public string RoleName { get; set; }
        public string PermissionIds { get; set; }
        public string ModificationReason { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(RoleName))
            {
                yield return new( GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_RequiredErrorMessage"), new[] { nameof(RoleName) });
            }

            if (string.IsNullOrEmpty(PermissionIds))
            {
                yield return new( GeneralLibrary.Utilities.Language.GetString("AlertAndMessage_RequiredErrorMessage"), new[] { nameof(PermissionIds) });
            }
        }
    }
}
