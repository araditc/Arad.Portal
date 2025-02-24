using System;
using System.Collections.Generic;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using Arad.Portal.DataLayer.Models.Shared.User;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.Models.Shared.Role;
using Arad.Portal.Models.Shared.Validator;

namespace Arad.Portal.Models.Shared.User;

public class UserDto
{
    public UserDto()
    {
        Addresses = new();
        //FavoriteList = new();
        DomainId = new();
        Roles = new();
    }

    public string UserId { get; set; }

    [ErrorMessage("AlertAndMessage_UserNameRequired")]
    public string UserName { get; set; }
    public bool IsSystemAccount { get; set; }
    public bool IsDomainAdmin { get; set; }
    public bool IsActive { get; set; }
    public bool IsVendor { get; set; }

    [MobilePhoneUserDtoValidator]
    [ErrorMessage("AlertAndMessage_PhonenumberRequired")]
    public string PhoneNumber { get; set; }

    public string FullMobile { get; set; }

    [ErrorMessage("AlertAndMessage_PhonenumberRequired")]
    public string FirstName { get; set; }

    [ErrorMessage("AlertAndMessage_LastNameRequired")]
    public string LastName { get; set; }
    public Profile UserProfile { get; set; }
    public List<Address> Addresses { get; set; }

    [ErrorMessage("AlertAndMessage_UserRoleRequired")]
    public string UserRoleId { get; set; }
    public OTP Otp { get; set; }
    public bool IsDeleted { get; set; }
    //public List<string> FavoriteList { get; set; }
    public List<string> DomainId { get; set; }
    public DateTime CreationDate { get; set; }
    public string CreatorId { get; set; }
    public string CreatorUserName { get; set; }
    public DateTime LastLoginDate { get; set; }

    public List<RoleListView> Roles { get; set; }

}