using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class IrankishModel
    {
        public string BaseUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string MerchantId { get; set; }
        public string TerminalId { get; set; }
        public string AcceptorId { get; set; }
        public string AccountIban { get; set; }
        public string Sha1 { get; set; }
    }
}
