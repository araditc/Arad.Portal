using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class RequestMenuModel
    {
        public string PathString { get; set; }
        public string Domain { get; set; }
        public string RoleId { get; set; }
    }
}
