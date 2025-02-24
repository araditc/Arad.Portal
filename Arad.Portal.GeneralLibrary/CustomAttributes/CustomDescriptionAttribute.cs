using Arad.Portal.GeneralLibrary.Utilities;

using System.ComponentModel;

namespace Arad.Portal.GeneralLibrary.CustomAttributes;

public class CustomDescriptionAttribute : DescriptionAttribute
{
    public CustomDescriptionAttribute(string value)
        : base(GetMessageFromResource(value))
    { }

    private static string GetMessageFromResource(string value)
    {
        return UtilityLanguage.GetString(value);
    }
}