using Arad.Portal.GeneralLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.GeneralLibrary.CustomAttributes
{
    public class CustomDescriptionAttribute : DescriptionAttribute
    {
        public CustomDescriptionAttribute(string value)
                : base(GetMessageFromResource(value))
        { }

        private static string GetMessageFromResource(string value)
        {
            return Language.GetString(value);
        }
    }
}
