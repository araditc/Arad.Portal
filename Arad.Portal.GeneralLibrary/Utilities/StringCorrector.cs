//
//  --------------------------------------------------------------------
//  Copyright (c) 2005-2021 Arad ITC.
//
//  Author : Davood Ghashghaei <ghashghaei@arad-itc.org>
//  Licensed under the Apache License, Version 2.0 (the "License")
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0 
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  --------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.GeneralLibrary.Utilities
{
    public static class StringCorrector
    {
        public static string GetUri(string scheme, string host)
        {
            string u = $"{scheme}://{host}";
            return u;
        }


        public static string ToPersianString(this string oldString)
        {
            var result = oldString
                .Replace("0", "۰")
                .Replace("1", "۱")
                .Replace("2", "٢")
                .Replace("3", "۳")
                .Replace("4", "۴")
                .Replace("٥", "۵")
                .Replace("6", "٦")
                .Replace("7", "٧")
                .Replace("8", "٨")
                .Replace("9", "۹")
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
                .Replace("۲", "2")
                .Replace("۳", "3")
                .Replace("۴", "4")
                .Replace("۵", "5")
                .Replace("۶", "6")
                .Replace("۷", "7")
                .Replace("۸", "8")
                .Replace("۹", "9");
            return result;
        }
    }
}
