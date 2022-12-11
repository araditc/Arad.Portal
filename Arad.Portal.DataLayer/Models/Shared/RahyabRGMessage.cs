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

        public string MessageId { get; set; }
    }
}
