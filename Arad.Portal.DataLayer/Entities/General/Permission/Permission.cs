using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Entities.General.Permission
{
    public class Permission : BaseEntity
    {
        public string PermissionId { get; set; }
        public string Title { get; set; }
        public List<string> Routes { get; set; }
        public string ClientAddress { get; set; }
        public Enums.PermissionType Type { get; set; }
        public Enums.PermissionMethod Method { get; set; }

        public string MenuId { get; set; }

        /// <summary>
        /// ParentMenu
        /// </summary>
        public string ParentMenuId { get; set; }
        public double Priority { get; set; }
        public string Icon { get; set; }
    }

}
