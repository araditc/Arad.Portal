using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class SamanModel
    {
        public string BaseAddress { get; set; }
        public string TokenEndPoint { get; set; }
        public string GatewayEndPoint { get; set; }
        public string VerifyEndpoint { get; set; }

        public string ReverseEndPoint { get; set; }
        public string MerchantId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string TerminalId { get; set; }

        
    }
}
