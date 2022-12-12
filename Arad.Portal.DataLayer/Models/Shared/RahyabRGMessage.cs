using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class RahyabRGMessage
    {
        public List<SmsLikeToLikeMessage> ListLikeToLikeMessage { get; set; } = new();

        public string Number { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Company { get; set; }
    }

    public class SmsLikeToLikeMessage
    {
        public string DestNumber { get; set; }

        public string Message { get; set; }
    }

    public class SmsEndPointConfig
    {

        public string LineNumber { get; set; }

        public string Endpoint { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Company { get; set; }

        public string TokenEndpoint { get; set; }

        public string TokenUserName { get; set; }

        public string TokenPassword { get; set; }
    }

   
}
