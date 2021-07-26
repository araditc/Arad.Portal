using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Role
{
    public class RoleDTO
    {
        public RoleDTO()
        {
            PermissionIds = new();
        }
        public string RoleId { get; set; }

        [Required(ErrorMessage = "لطفا نام نقش را وارد نمایید.")]
        public string RoleName { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsActive { get; set; }

        public string CreatorId { get; set; }

        public string CreatorUserName { get; set; }

        public bool HasModifications { get; set; }

        public bool IsDeleted { get; set; }

        [Required(ErrorMessage = "لطفا دسترسی های نقش را انتخاب نمایید.")]
        public List<string> PermissionIds { get; set; }
        public string ModificationReason { get; set; }
    }
}
