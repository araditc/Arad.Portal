using System;
using System.Collections.Generic;
using System.Text;

namespace Arad.Portal.DataLayer.Models.Shared
{ 
    public static class StringCorrector
    {
        public static string ToPersianString(this string oldString)
        {
            var result = oldString
                .Replace("0", "۰")
                .Replace("1", "۱")
                .Replace("3", "۳")
                .Replace("4", "۴")
                .Replace("9", "۹")
                .Replace("2", "٢")
                .Replace("5", "۵")
                .Replace("6", "۶")
                .Replace("7", "٧")
                .Replace("8", "٨")
                .Replace('ي', 'ی')
                .Replace('ك', 'ک')
                .Replace("'", "`");
            return result;
        }

        public static string ToEngishString(this string oldString)
        {

            var result = oldString
                .Replace("۰", "0")
                .Replace("۱", "1")
                .Replace("۳", "3")
                .Replace("۴", "4")
                .Replace("۹", "9")
                .Replace("۲", "2")
                .Replace("۵", "5")
                .Replace("۶", "6")
                .Replace("۷", "7")
                .Replace("۸", "8");
            return result;
        }
    }
}
