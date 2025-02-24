using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Arad.Portal.Helpers.UI;

public static class Descriptor
{
    public static string GetDescription<T>(this T e) where T : IConvertible
    {
        if (e is Enum)
        {
            Type type = e.GetType();
            Array values = Enum.GetValues(type);

            foreach (int val in values)
            {
                if (val == e.ToInt32(CultureInfo.InvariantCulture))
                {
                    MemberInfo[] memInfo = type.GetMember(type.GetEnumName(val));
                    DescriptionAttribute descriptionAttribute = memInfo[0]
                                                                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                                                                .FirstOrDefault() as DescriptionAttribute;

                    if (descriptionAttribute != null)
                    {
                        return descriptionAttribute.Description;
                    }
                }
            }
        }
        return null; // could also return string.Empty
    }

}