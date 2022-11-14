using Arad.Portal.GeneralLibrary.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class ChangePassword
    {
        [ErrorMessage("AlertAndMessage_PasswordRequired")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
       
        [ErrorMessage("AlertAndMessage_NewPasswordRequired")]
        public string NewPassword { get; set; }

       
        [ErrorMessage("AlertAndMessage_ReNewPasswordRequired")]
        public string ReNewPassword { get; set; }
    }
}
