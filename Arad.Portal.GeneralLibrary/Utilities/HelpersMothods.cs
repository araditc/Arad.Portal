using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Arad.Portal.GeneralLibrary.Utilities
{
    public class HelpersMethods
    {

        public static string GetDirection(CultureInfo currentCulture)
        {
            string direction = "";
            if (currentCulture.TextInfo.IsRightToLeft)
            {
                direction = "rtl";

            }
            else
            {
                direction = "ltr";

            }
            return direction;
        }

        public static string GetHtmlLang(CultureInfo currentCulture)
        {
            string lang = "";
            if (currentCulture.Name.Contains("-"))
            {
                lang = currentCulture.Name.Split("-")[0];
            }
            else
            {
                lang = currentCulture.Name;
            }
            return lang;
        }
    }
}
