using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Permission
{
    public class PerSelect
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public bool IsSelected { get; set; }
        //public Enums.PermissionType Type { get; set; }
        //public Enums.PermissionMethod Method { get; set; }
    }
}
