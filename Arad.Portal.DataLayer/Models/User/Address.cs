using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.CustomAttributes;


namespace Arad.Portal.DataLayer.Models.User
{
    public class Address
    {
        public string Id { get; set; }
        public bool IsMainAddress { get; set; }

        public string Province { get; set; }

        public string ProvinceName { get; set; }

        [CustomErrorMessage("AlertAndMessage_SelectCity")]
        public string TownShip { get; set; }

        public string TownShipName { get; set; }

        [CustomErrorMessage("AlertAndMessage_RequiredErrorMessage")]
        public string PostalCode { get; set; }

        public string Street { get; set; }

        public string Street2 { get; set; }

        public string HousePhoneNumber { get; set; }
       
        //[Range(1, int.MaxValue, ErrorMessage = "لطفا شماره پلاک معتبر وارد نمایید.")]
        public int? Plaque { get; set; }

        public string Floor { get; set; }
        /// <summary>
        /// واحد
        /// </summary>
        public string SideFloor { get; set; }

        public string BuildingName { get; set; }

        public string Description { get; set; }

        public AddressType AddressType { get; set; }
    }
    /// <summary>
    /// بهتر است به جای enum یک آبجکت از دیتابیس باشه
    /// </summary>
    public enum AddressType
    {
        BillingAddress,
        ShippingAddress,
        WorkAddress,
        HomeAddress
    }
}
