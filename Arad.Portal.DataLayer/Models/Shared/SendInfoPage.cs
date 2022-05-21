using Arad.Portal.DataLayer.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class SendInfoPage
    {
        public List<Address> Addresses { get; set; }
        public string TotalCost { get; set; }

        public string CurrencySymbol { get; set; }

        public string UserCartId { get; set; }
    }
}
