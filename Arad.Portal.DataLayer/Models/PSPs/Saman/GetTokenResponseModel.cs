using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.PSPs.Saman
{
    public class GetTokenResponseModel
    {
        public int Status { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string Token { get; set; }
    }
}
