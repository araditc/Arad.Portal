using Arad.Portal.DataLayer.Models.Validator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;
using System.Text;

namespace Arad.Portal.DataLayer.Models.User
{
    public class RegisterUserModel
    {
        //public RegisterUserModel()
        //{
        //    UserRoles = new();
        //}
        [Required(ErrorMessage = "ورود نام کاربری الزامی می باشد.")]
        [MinLength(3, ErrorMessage = "طول نام کاربری نباید کم تر از 3 کاراکتر باشد.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "لطفا نام کاربر را وارد نمایید.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "لطفا نام خانوادگی کاربر را وارد نمایید.")]
        public string LastName { get; set; }

        [MobilePhoneValidator]
        [Required(ErrorMessage = "لطفا شماره موبایل را وارد نمایید.")]
        public string PhoneNumber { get; set; }

        public string FullMobile { get; set; }

        [Required(ErrorMessage = "وضعیت کاربر را مشخص نمایید.")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "لطفا برای کاربر نقشی را انتخاب نمایید.")]
        public string UserRoleId { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "لطفا رمز عبور را وارد نمایید.")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*[0-9]).{4,}$", ErrorMessage = "پسورد باید ترکیبی از حروف وعدد باشد")]
        [MinLength(6, ErrorMessage = "طول پسورد نباید کم تر از 6 کاراکتر باشد.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "تکرار کلمه عبور الزامی می باشد.")]
        [Compare("Password",ErrorMessage = " کلمه عبور و تکرار کلمه عبور باید یکسان باشند. ")]
        public string RePassword { get; set; }
       
    }
}
