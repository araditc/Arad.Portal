using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public class OperationResult
    {
        public bool Succeeded { get; set; } = false;

        public string Message { get; set; }

        public string Url { get; set; }
    }
}
