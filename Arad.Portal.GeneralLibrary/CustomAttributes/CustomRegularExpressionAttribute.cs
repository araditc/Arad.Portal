using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arad.Portal.GeneralLibrary.Utilities;
using System.Text.RegularExpressions;

namespace Arad.Portal.GeneralLibrary.CustomAttributes
{
    public class CustomRegularExpressionAttribute : RegularExpressionAttribute
    {
        public CustomRegularExpressionAttribute(string pattern, string key):base(pattern)
        {
            Key = key;
        }
        public string Key { get; }
        public override string FormatErrorMessage(string name)
        {
            return Language.GetString(Key);
        }

        public override bool IsValid(object value)
        {
            return Regex.IsMatch(value.ToString(), Pattern);
        }
    }
}
