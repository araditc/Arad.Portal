using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.CustomAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Permission
{
    public class UserPermissionViewModel
    {
        public string ModificationReason { get; set; }
        public bool IsEditView { get; set; }

        public double Priority { get; set; }
        public string Id { get; set; }

        //[CustomErrorMessage("AlertAndMessage_FieldEssential")]
        //public Enums.PermissionType Type { get; set; }
        //[CustomErrorMessage("AlertAndMessage_FieldEssential")]
        //public Enums.PermissionMethod Method { get; set; }

        [ErrorMessage("AlertAndMessage_FieldEssential")]
        public string Title { get; set; }

        public string MenuIdOfModule { get; set; }

        public string ParentMenuId { get; set; }
        public string Routes { get; set; }
        public string Icon { get; set; }
        public string ClientAddress { get; set; }
       
    }
}
