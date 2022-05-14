using Arad.Portal.DataLayer.Models.Validator;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.CustomAttributes;

namespace Arad.Portal.DataLayer.Models.User
{
    public class UserEdit
    {
      
        public string Id { get; set; }

        [CustomDisplayName("User_FirstName")]
        [CustomErrorMessage("AlertAndMessage_NameRequired")]
        public string FirstName { get; set; }

        [CustomDisplayName("User_LastName")]
        [CustomErrorMessage("AlertAndMessage_LastName")]
        public string LastName { get; set; }

        [MobilePhoneUserEditValidator]
        [CustomDisplayName("User_PhoneNumber")]
        //[Required(ErrorMessage = "لطفا شماره موبایل را وارد نمایید.")]
        [CustomErrorMessage("AlertAndMessage_PhoneNumberRequired")]
        public string PhoneNumber { get; set; }

        //[CustomDisplayName("IsVendor")]
        public bool IsVendor { get; set; }

        [CustomDisplayName("User_FullMobile")]
        public string FullMobile { get; set; }

        [CustomDisplayName("User_Role")]
        [CustomErrorMessage("AlertAndMessage_UserRoleRequired")]
        public string UserRoleId { get; set; }

        [CustomDisplayName("DefaultLanguage")]
        [CustomErrorMessage("AlertAndMessage_DefaultLanguageRequired")]
        public string DefaultLanguageId { get; set; }

        [CustomDisplayName("DefaultLanguage")]
        public string DefaultLanguageName { get; set; }

        [CustomDisplayName("DefaultCurrency")]
        public string DefaultCurrencyId { get; set; }

        [CustomDisplayName("DefaultCurrency")]
        public string DefaultCurrencyName { get; set; }
    }
}
