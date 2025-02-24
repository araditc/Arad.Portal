using Arad.Portal.GeneralLibrary.Utilities;

using System.ComponentModel.DataAnnotations;

namespace Arad.Portal.GeneralLibrary.CustomAttributes;

public class CustomMinLengthAttribute : MinLengthAttribute
{
    public CustomMinLengthAttribute(int length, string key):base(length)
    {
        Key = key;
    }
    public string Key { get; }

    public override string FormatErrorMessage(string name)
    {
        string msg = UtilityLanguage.GetString(Key);
        msg = msg.Replace("0", name).Replace("1", Length.ToString());
        return msg;
    }
    public override bool IsValid(object value)
    {
        if (value != null)
        {
            return value.ToString().Length >= Length;
        }
        else
            return false;
    }
}