using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class LoginViewModel
    {
        public string ReturnUrl { get; set; }

        [Required(ErrorMessage = "لطفا نام کاربری را وارد کنید.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "لطفا رمز عبور را وارد کنید.")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string Captcha { get; set; }
    }
}
