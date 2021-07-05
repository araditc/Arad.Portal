using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.User;
using AspNetCore.Identity.Mongo.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.User
{
    public class ApplicationUser : MongoUser<string>
    {
        public ApplicationUser()
        {
            Addresses = new ();
            FavoriteList = new ();
            DomainId = new ();
            LoginData = new (); 
        }
        public bool IsSystemAccount { get; set; }
        public bool IsDomainAdmin { get; set; }
        public bool IsActive { get; set; }
        public Profile Profile { get; set; }
        public List<Address> Addresses { get; set; }
        public List<string> UserRoles { get; set; }      
        public OneTimePass Otp { get; set; }
        public bool IsDeleted { get; set; }
        public List<string> FavoriteList { get; set; }
        public List<string> DomainId { get; set; } 
        public DateTime CreationDate { get; set; }
        public string CreatorId { get; set; }
        public string CreatorUserName { get; set; }
        public List<Modification> Modifications { get; set; }
        public DateTime LastLoginDate { get; set; }
        public List<LoginLogoutRecord> LoginData { get; set; } = new List<LoginLogoutRecord>();
    }


    public class ApplicationRole : MongoRole<string>
    {

    }

    //public enum AccountType
    //{
    //    [Description("کاربر سیستمی")]
    //    System = 0,

    //    [Description("سوپرادمین")]
    //    SuperAdmin = 1,

    //    [Description("کاربر ادمین")]
    //    Admin = 2,

    //    [Description("کاربر عادی")]
    //    User = 3
    //}
}
