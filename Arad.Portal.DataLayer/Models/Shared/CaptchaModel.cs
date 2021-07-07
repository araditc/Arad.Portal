using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class CaptchaModel
    {
        public string Code { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
