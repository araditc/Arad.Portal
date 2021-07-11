using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers.SMS
{
    public class SMS
    {
        public string SenderId { get; set; }
        public string Receivers { get; set; }
        public string SmsText { get; set; }

        public string AradVas_Link_1 { get; set; }
        public string AradVas_UserName { get; set; }
        public string AradVas_Password { get; set; }
        public string AradVas_Number { get; set; }
    }
}