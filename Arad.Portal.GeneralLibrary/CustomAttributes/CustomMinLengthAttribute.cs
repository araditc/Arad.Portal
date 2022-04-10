using Arad.Portal.GeneralLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.GeneralLibrary.CustomAttributes
{
    public class CustomMinLengthAttribute : MinLengthAttribute
    {
        public CustomMinLengthAttribute(int length, string key):base(length)
        {
            Key = key;
        }
        public string Key { get; }

        public override string FormatErrorMessage(string name)
        {
            var msg = Language.GetString(Key);
            msg = msg.Replace("0", name).Replace("1", Length.ToString());
            return msg;
        }
        public override bool IsValid(object value)
        {
            return value.ToString().Length >= Length;
        }
    }
}
