using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class PaymentModel
    {
        public string Address { get; set; }
        public string UserCartId { get; set; }

        public string PspType { get; set; }

        public int PspTypeId { get; set; }
    }
}
