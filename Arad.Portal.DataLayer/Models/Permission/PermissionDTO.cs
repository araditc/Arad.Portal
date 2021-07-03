using Arad.Portal.DataLayer.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Permission
{
    public class PermissionDTO
    {
        public string PermissionId { get; set; }
        public string Title { get; set; }
        public string Routes { get; set; }
        //public string Route { get; set; }
        public string ClientAddress { get; set; }
        public Enums.PermissionType Type { get; set; }
        public Enums.PermissionMethod Method { get; set; }

        public string MenuIdOfModule { get; set; }

        /// <summary>
        /// ParentMenu
        /// </summary>
        public string ParentMenuId { get; set; }
        public double Priority { get; set; }
        public string Icon { get; set; }
        public bool IsEditView { get; set; }
        public string ModificationReason { get; set; }
    }
}
