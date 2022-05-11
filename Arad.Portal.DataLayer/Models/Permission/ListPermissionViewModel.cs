using Arad.Portal.DataLayer.Entities.General.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.DataLayer.Models.Permission
{
    public class ListPermissionViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        //public Enums.PermissionType Type { get; set; }
        //public Enums.PermissionMethod Method { get; set; }
        public string ClientAddress { get; set; }
        public string Routes { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatorName { get; set; }
        public bool HasModification { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
