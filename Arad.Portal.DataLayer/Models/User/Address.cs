using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.CustomAttributes;


namespace Arad.Portal.DataLayer.Models.User
{
    public class Address
    {
        public string Id { get; set; }
        public AddressType AddressType { get; set; }
        public bool IsMain { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }

        [CustomDisplayName("Profile_Province")]
        public string ProvinceId { get; set; }
        public string ProvinceName { get; set; }

        [CustomDisplayName("Profile_City")]
        public string CityId { get; set; }
        public string CityName { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        [CustomDisplayName("Profile_ZipCode")]/*ZipCode*/
        public string PostalCode { get; set; }
       
    }
    /// <summary>
    /// TODO : بهتر است به جای enum یک آبجکت از دیتابیس باشه
    /// </summary>
    public enum AddressType
    {
        BillingAddress,
        ShippingAddress,
        WorkAddress,
        HomeAddress
    }
}
