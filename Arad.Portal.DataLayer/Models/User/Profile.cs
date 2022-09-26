using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class Profile
    {
        public Profile()
        {
            Addresses = new();
            ProfilePhoto = new();
            BankAccount = new();
            Access = new();
            InformProductList = new();
        }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public string FatherName { get; set; }

        public Gender Gender { get; set; }

        public List<Address> Addresses { get; set; }

        //???
        public string NationalCode { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime BirthDate { get; set; }

        public Image ProfilePhoto { get; set; }

        public string CompanyName { get; set; }

        public List<string> InformProductList { get; set; }

        public BankAccount BankAccount { get; set; }

        public UserType UserType { get; set; }
                 
        public SpecialAccess Access { get; set; }

        public string DefaultCurrencyId { get; set; }

        public string  DefaultCurrencyName { get; set; }

        public string DefaultLanguageId { get; set; }

        public string DefaultLanguageName { get; set; }
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
        public string AccountNumber { get; set; }
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
            AccessibleContentCategoryIds = new();
        }
        /// <summary>
        /// if user is type of seller then also has a role seller
        /// and this Ids is the Id of productGroups which product can be added by this user
        /// </summary>
        public List<string> AccessibleProductGroupIds { get; set; }

        public List<string> AccessibleContentCategoryIds { get; set; }
    }

    public enum Gender
    {
        [CustomDescription("EnumDesc_Male")]
        Male,
        [CustomDescription("EnumDesc_Female")]
        Female
    }
}
