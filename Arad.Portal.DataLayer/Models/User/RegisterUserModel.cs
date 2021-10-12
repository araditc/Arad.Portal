using Arad.Portal.DataLayer.Models.Role;
using Arad.Portal.DataLayer.Models.Validator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Arad.Portal.GeneralLibrary.Utilities;
using System.Security.AccessControl;
using System.Text;
using Arad.Portal.GeneralLibrary.CustomAttributes;

namespace Arad.Portal.DataLayer.Models.User
{
    public class RegisterUserModel
    {
        public RegisterUserModel()
        {
            Roles = new List<RoleListView>();
        }
        
        [CustomErrorMessage("AlertAndMessage_UserNameRequired")]
        [CustomMinLength(3, "AlertAndMessage_MinLength")]
        public string UserName { get; set; }

        [CustomErrorMessage("AlertAndMessage_NameRequired")]
        public string Name { get; set; }

        [CustomErrorMessage("AlertAndMessage_LastName")]
        public string LastName { get; set; }

        public string FatherName { get; set; }
       
        public Gender Gender { get; set; }

        [MobilePhoneValidator]
        [CustomErrorMessage("AlertAndMessage_PhoneNumberRequired")]
        public string PhoneNumber { get; set; }

        public string FullMobile { get; set; }


        [CustomErrorMessage("AlertAndMessage_DefaultLanguageRequired")]
        public string DefaultLanguageId { get; set; }

        public string DefaultLanguageName { get; set; }

        public string DefaultCurrencyId { get; set; }

        public string DefaultCurrencyName { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsVendor { get; set; }

        public string DomainId { get; set; }

        [CustomErrorMessage("AlertAndMessage_UserRoleRequired")]
        public string UserRoleId { get; set; }

        [DataType(DataType.Password)]
        [CustomErrorMessage("AlertAndMessage_PasswordRequired")]
        [CustomRegularExpression("^(?=.*[A-Za-z])(?=.*[0-9]).{4,}$", "AlertAndMessage_PasswordValidation")]

        [CustomMinLength(6, "AlertAndMessage_MinLength")]
        public string Password { get; set; }

        [CustomErrorMessage("AlertAndMessage_RePasswordRequired")]
        //[CustomCompare("Password", "AlertAndMessage_PasswordRepassWordCompare")]
        public string RePassword { get; set; }

        public List<RoleListView> Roles { get; set; }

    }
}
