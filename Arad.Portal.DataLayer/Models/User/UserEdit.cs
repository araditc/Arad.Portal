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

        [CustomErrorMessage("AlertAndMessage_NameRequired")]
        public string FirstName { get; set; }

        [CustomErrorMessage("AlertAndMessage_LastName")]
        public string LastName { get; set; }

        [MobilePhoneUserEditValidator]
        [Required(ErrorMessage = "لطفا شماره موبایل را وارد نمایید.")]
        [CustomErrorMessage("AlertAndMessage_PhoneNumberRequired")]
        public string PhoneNumber { get; set; }

        public string FullMobile { get; set; }

        [CustomErrorMessage("AlertAndMessage_UserRoleRequired")]
        public string UserRoleId { get; set; }
    }
}
