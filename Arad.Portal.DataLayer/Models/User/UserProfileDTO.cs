using Arad.Portal.DataLayer.Models.Shared;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class UserProfileDTO
    {
        public UserProfileDTO()
        {
            Addresses = new();
        }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string FatherName { get; set; }
        public Gender Gender { get; set; }
        public List<Address> Addresses { get; set; }
        //???
        public string NationalCode { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? BirthDate { get; set; }
        public string PersianBirthDate { get; set; }
        public Image ProfilePhoto { get; set; }
        public string CompanyName { get; set; }
        public BankAccount BankAccount { get; set; }
        public UserType UserType { get; set; }
        public SpecialAccess Access { get; set; }
        public string DefaultCurrencyId { get; set; }
        public string DefaultCurrencyName { get; set; }
        public string DefaultLanguageId { get; set; }
        public string DefaultLanguageName { get; set; }
        public string FileContent { get; set; }
        public string FileName { get; set; }
    }
}
