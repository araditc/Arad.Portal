using Arad.Portal.GeneralLibrary.CustomAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class UserChangePasswordModel
    {
        public string UserId { get; set; }

        [ErrorMessage("AlertAndMessages_InputRequiredErrorMessage")]
        public string OldPass { get; set; }

        [ErrorMessage("AlertAndMessages_InputRequiredErrorMessage")]
        public string NewPass { get; set; }

        [ErrorMessage("AlertAndMessages_InputRequiredErrorMessage")]
        public string RepNewPass { get; set; }
    }
}
