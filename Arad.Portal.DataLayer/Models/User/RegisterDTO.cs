using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class RegisterDTO
    {
        public string ReturnUrl { get; set; }

        public string CellPhoneNumber { get; set; }

        public string FullCellPhoneNumber { get; set; }

        public string Email { get; set; }

        public string Username { get; set; }
        public string Captcha { get; set; }

        public string SecurityCode { get; set; }

        public string CurrentPass { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "AlertAndMessage_PasswordRequired")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*[0-9]).{4,}$", ErrorMessage = "AlertAndMessage_PasswordValidation")]
        [MinLength(6, ErrorMessage = "AlertAndMessage_MinLength")]
        public string NewPass { get; set; }

        [Required(ErrorMessage = "AlertAndMessage_RePasswordRequired")]
        [Compare(nameof(NewPass), ErrorMessage = "AlertAndMessage_PasswordRepassWordCompare")]
        public string ReNewPass { get; set; }
    }
}
