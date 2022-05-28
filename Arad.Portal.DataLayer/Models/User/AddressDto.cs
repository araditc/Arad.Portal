using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class AddressDto
    {
        public string Id { get; set; }
        public AddressType AddressType { get; set; }

        public string AddressTypeId { get; set; }
        public bool IsMain { get; set; }
        public string CountryId { get; set; }
        public string CountryName { get; set; }

       
        public string ProvinceId { get; set; }
        public string ProvinceName { get; set; }

       
        public string CityId { get; set; }
        public string CityName { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

      
        public string PostalCode { get; set; }

    }
}
