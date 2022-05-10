using System;
using System.ComponentModel.DataAnnotations;

namespace Arad.Portal.GeneralLibrary.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CustomIntegerAttribute : DataTypeAttribute
    {
        public CustomIntegerAttribute()
            : base("integer")
        {
        }

        public override string FormatErrorMessage(string name)
        {
            if (ErrorMessage == null && ErrorMessageResourceName == null)
            {
                ErrorMessage = "ValidatorResources.IntegerAttribute_Invalid";
            }

            return base.FormatErrorMessage(name);
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true;

            int retNum;

            return int.TryParse(Convert.ToString(value), out retNum);
        }
    }
}