using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.User;
using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson.Serialization.Attributes;

using MongoDbGenericRepository.Attributes;

namespace Arad.Portal.DataLayer.Entities.General.User
{
    /// <summary>
    /// all users of site whether they are site user or admin user will be store here 
    /// this entity inherit from mongoUser which inherit identityUser 
    /// </summary>
    /// 
    [MongoDbGenericRepository.Attributes.CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<string>
    {
        public ApplicationUser()
        {
            LoginData = new();
            Profile = new();
            LoginData = new();
            Otp = new();
            Domains = new();
            Modifications = new();
        }
        /// <summary>
        /// if isSystemAccount = true this user have full access to all links and full ability only one user have this ability
        /// </summary>
        public bool IsSystemAccount { get; set; }

        
        /// <summary>
        /// inactive user cant login and work with its account
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// if user constructed in site this field is true
        /// </summary>
        public bool IsSiteUser { get; set; }

        /// <summary>
        /// some information of user
        /// </summary>
        public Profile Profile { get; set; }

        /// <summary>
        /// the primary key of Role which this user have
        /// </summary>
        public string UserRoleId { get; set; }

        /// <summary>
        /// one time password which send for registeration and changing pass of user
        /// </summary>
        public OTP Otp { get; set; }

        /// <summary>
        /// we have soft deleted all deleted entities store with isdeleted = true
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// all domains which user belong to whether domainOwner or not
        /// </summary>
         public List<UserDomain> Domains { get; set; }
        //public string DomainId { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreationDate { get; set; }
        public string CreatorId { get; set; }
        public string CreatorUserName { get; set; }
        public List<Modification> Modifications { get; set; }
        public DateTime LastLoginDate { get; set; }
        public List<LoginLogoutRecord> LoginData { get; set; } 
    }


    public class UserDomain
    {
        public string DomainId { get; set; }

        public string DomainName { get; set; }

        public bool IsOwner { get; set; }
    }

    public class ApplicationRole : MongoIdentityRole<string>
    {

    }

}
