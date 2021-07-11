using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Validator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class UserDTO
    {
        public UserDTO()
        {
            Addresses = new();
            FavoriteList = new();
            DomainId = new();
        }

        [Required(ErrorMessage = "شناسه کاربر را مشخص نمایید")]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsSystemAccount { get; set; }
        public bool IsDomainAdmin { get; set; }
        public bool IsActive { get; set; }

        [MobilePhoneUserDTOValidator]
        [Required(ErrorMessage = "لطفا شماره موبایل را وارد نمایید.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "لطفا نام کاربر را وارد نمایید.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "لطفا نام خانوادگی کاربر را وارد نمایید.")]
        public string LastName { get; set; }
        public Profile UserProfile { get; set; }
        public List<Address> Addresses { get; set; }

        [Required(ErrorMessage = "لطفا برای کاربر نقشی را انتخاب نمایید.")]
        public List<string> UserRoles { get; set; }
        public OneTimePass Otp { get; set; }
        public bool IsDeleted { get; set; }
        public List<string> FavoriteList { get; set; }
        public List<string> DomainId { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatorId { get; set; }
        public string CreatorUserName { get; set; }
        public DateTime LastLoginDate { get; set; }
        
    }
}
