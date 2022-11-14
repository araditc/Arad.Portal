using Arad.Portal.GeneralLibrary.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.User
{
    public class UserProfile
    {
        [ErrorMessage("AlertAndMessage_NameRequired")]
        public string Name { get; set; }

        [ErrorMessage("AlertAndMessage_LastName")]
        public string LastName { get; set; }
    }
}
