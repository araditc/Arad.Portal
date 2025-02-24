using System.ComponentModel.DataAnnotations;

using Arad.Portal.GeneralLibrary.Utilities;
using System.Text.RegularExpressions;

namespace Arad.Portal.GeneralLibrary.CustomAttributes;

public class CustomRegularExpressionAttribute : RegularExpressionAttribute
{
    public CustomRegularExpressionAttribute(string pattern, string key):base(pattern)
    {
        Key = key;
    }
    public string Key { get; }
    public override string FormatErrorMessage(string name)
    {
        return UtilityLanguage.GetString(Key);
    }

    public override bool IsValid(object value)
    {
        return Regex.IsMatch(value.ToString(), Pattern);
    }
}