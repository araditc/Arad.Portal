using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class AjaxValidationErrorModel
    {
        public string Key { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
    }
}
