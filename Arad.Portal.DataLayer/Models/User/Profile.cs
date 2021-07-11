using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class Profile
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FatherName { get; set; }

        public Gender Gender { get; set; }

        public string NationalId { get; set; }

        //public string PhoneNumber { get; set; }

        public DateTime BirthDate { get; set; }

        public Picture ProfilePhoto { get; set; }

        public string CompanyName { get; set; }

        public BankAccount BankAccount { get; set; }

        public UserType UserType { get; set; }
                 
        public SpecialAccess Access { get; set; }
    }


    /// <summary>
    /// this part uses in Admin (dashboard)
    /// </summary>
    public enum UserType
    {
        Admin,
        Seller,
        Customer
    }
    public class BankAccount
    {
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string Iban { get; set; }
        public string BankName { get; set; }
        public bool IsApproved { get; set; }
    }

    public class SpecialAccess
    {
        public SpecialAccess()
        {
            AccessibleProductGroupIds = new();

        }
        /// <summary>
        /// if user is type of seller then also has a role seller
        /// and this Ids is the Id of productGroups which product can be added by this user
        /// </summary>
        public List<string> AccessibleProductGroupIds { get; set; }
    }

    public enum Gender
    {
      Men,
      Women
    }
}
