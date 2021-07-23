using Arad.Portal.DataLayer.Models.Validator;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class UserEdit
    {
       
        [Required(ErrorMessage = "شناسه کاربر را مشخص نمایید")]
        public string Id { get; set; }

        [Required(ErrorMessage = "لطفا نام کاربر را وارد نمایید.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "لطفا نام خانوادگی کاربر را وارد نمایید.")]
        public string LastName { get; set; }

        [MobilePhoneUserEditValidator]
        [Required(ErrorMessage = "لطفا شماره موبایل را وارد نمایید.")]
        public string PhoneNumber { get; set; }

        public string FullMobile { get; set; }

        [Required(ErrorMessage = "لطفا برای کاربر نقشی را انتخاب نمایید.")]
        public string UserRoleId { get; set; }
    }
}
