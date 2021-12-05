using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class RegisterDTO
    {
        public string ReturnUrl { get; set; }

        public string CellPhoneNumber { get; set; }

        public string FullCellPhoneNumber { get; set; }

        public string Email { get; set; }

        public string Captcha { get; set; }

        public string SecurityCode { get; set; }
    }
}
