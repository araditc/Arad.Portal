using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class LoginLogoutRecord
    {
        public DateTime DateTime { get; set; }
        public string AdminId { get; set; }
        public string AdminUserName { get; set; }
        public LoginType Type { get; set; }
    }

    public enum LoginType
    {
        Login = 1,
        Logout = 2,
        AdminLogin = 3
    }
}
