using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.GeneralLibrary.Utilities
{
    public static class DictionaryHelper
    {
        /// <summary>
        /// Gets string value and trims if exists.
        /// If key not found, returns default value
        /// </summary>
        internal static string GetStringValue(this Dictionary<string, string> dictionary, string key)
        {
            string value;
            bool found = dictionary.TryGetValue(key, out value);
            return found ? value.Trim() : null;
        }

        public static Dictionary<string, object> GetEnumAsDictionary<TEnum>() where TEnum : System.Enum
        {
            var _fields = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            return _fields.ToDictionary(t => t.Name, t => t.GetRawConstantValue());
        }
    }
}
