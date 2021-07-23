using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Validator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using Arad.Portal.DataLayer.Models.Role;

namespace Arad.Portal.DataLayer.Models.User
{
    public class UserDTO
    {
        public UserDTO()
        {
            Addresses = new();
            FavoriteList = new();
            DomainId = new();
            Roles = new();
        }

        public string UserId { get; set; }

        [CustomErrorMessage("AlertAndMessage_UserNameRequired")]
        public string UserName { get; set; }
        public bool IsSystemAccount { get; set; }
        public bool IsDomainAdmin { get; set; }
        public bool IsActive { get; set; }

        [MobilePhoneUserEditValidator]
        [CustomErrorMessage("AlertAndMessage_PhonenumberRequired")]
        public string PhoneNumber { get; set; }

        [CustomErrorMessage("AlertAndMessage_PhonenumberRequired")]
        public string FirstName { get; set; }

        [CustomErrorMessage("AlertAndMessage_LastNameRequired")]
        public string LastName { get; set; }
        public Profile UserProfile { get; set; }
        public List<Address> Addresses { get; set; }

        [CustomErrorMessage("AlertAndMessage_UserRoleRequired")]
        public string UserRoleId { get; set; }
        public OneTimePass Otp { get; set; }
        public bool IsDeleted { get; set; }
        public List<string> FavoriteList { get; set; }
        public List<string> DomainId { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatorId { get; set; }
        public string CreatorUserName { get; set; }
        public DateTime LastLoginDate { get; set; }

        public List<RoleListView> Roles { get; set; }

    }
}
