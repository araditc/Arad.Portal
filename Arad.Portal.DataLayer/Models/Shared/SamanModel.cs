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
    }
}
