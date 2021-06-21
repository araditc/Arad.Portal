using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
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
        public string UserId { get; set; }
        public string UserName { get; set; }
        public bool IsSystemAccount { get; set; }
        public bool IsDomainAdmin { get; set; }
        public bool IsActive { get; set; }
        public Profile UserProfile { get; set; }
        public List<Address> Addresses { get; set; }
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
