using Arad.Portal.DataLayer.Models.Role;
using Arad.Portal.DataLayer.Models.Validator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Arad.Portal.GeneralLibrary.Utilities;
using System.Security.AccessControl;
using System.Text;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using System.Net.Mail;

namespace Arad.Portal.DataLayer.Models.User
{
    public class RegisterUserModel
    {
        public RegisterUserModel()
        {
            Roles = new List<RoleListView>();
        }
        
        //[ErrorMessage("AlertAndMessage_UserNameRequired")]
        [Required(ErrorMessage = "AlertAndMessage_UserNameRequired")]
        [MinLength(3, ErrorMessage ="AlertAndMessage_MinLength")]
        public string UserName { get; set; }

        //[ErrorMessage("AlertAndMessage_NameRequired")]
        [Required(ErrorMessage = "AlertAndMessage_NameRequired")]
        public string Name { get; set; }

       // [ErrorMessage("AlertAndMessage_LastName")]
        [Required(ErrorMessage = "AlertAndMessage_LastName")]
        public string LastName { get; set; }

        public string FatherName { get; set; }
       
        public Gender Gender { get; set; }

        [MobilePhoneValidator]
        //[ErrorMessage("AlertAndMessage_PhoneNumberRequired")]
        //[Required(ErrorMessage = "AlertAndMessage_PhoneNumberRequired")]
        public string PhoneNumber { get; set; }

        public string FullMobile { get; set; }


        //[ErrorMessage("AlertAndMessage_DefaultLanguageRequired")]
        [Required(ErrorMessage = "AlertAndMessage_DefaultLanguageRequired")]
        public string DefaultLanguageId { get; set; }

        public string DefaultLanguageName { get; set; }

        [DataType(DataType.EmailAddress, ErrorMessage= "AlertAndMessage_EmailInvalid")]
        public string Email { get; set; }

        public string DefaultCurrencyId { get; set; }

        public string DefaultCurrencyName { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsVendor { get; set; }

        public bool  IsSiteUser { get; set; }

        [Required(ErrorMessage = "AlertAndMessage_DomainRequired")]
        public string DomainId { get; set; }

        //[ErrorMessage("AlertAndMessage_UserRoleRequired")]
        [Required(ErrorMessage = "AlertAndMessage_UserRoleRequired")]
        public string UserRoleId { get; set; }

        [DataType(DataType.Password)]
        //[ErrorMessage("AlertAndMessage_PasswordRequired")]
        [Required(ErrorMessage = "AlertAndMessage_PasswordRequired")]
        
        [RegularExpression("^(?=.*[A-Za-z])(?=.*[0-9]).{4,}$", ErrorMessage= "AlertAndMessage_PasswordValidation")]

        [MinLength(6, ErrorMessage = "AlertAndMessage_MinLength")]
        public string Password { get; set; }

        [Required(ErrorMessage = "AlertAndMessage_RePasswordRequired")]
        [Compare(nameof(Password), ErrorMessage = "AlertAndMessage_PasswordRepassWordCompare")]
        public string RePassword { get; set; }

        public List<RoleListView> Roles { get; set; }

    }
}
