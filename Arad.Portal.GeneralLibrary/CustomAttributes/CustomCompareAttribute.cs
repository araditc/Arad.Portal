using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Arad.Portal.GeneralLibrary.Utilities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.GeneralLibrary.CustomAttributes
{
    public class CustomCompareAttribute : CompareAttribute
    {
        public CustomCompareAttribute(string otherProperty, string key): base(otherProperty)
        {
            Key = key;
        }
        public string Key { get; }
        public override string FormatErrorMessage(string name)
        {
            return Language.GetString(Key);
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (OtherProperty == null || value == null || 
                (value != null && OtherProperty != null && value.ToString() != OtherProperty.ToString()))
            {
                return new ValidationResult(FormatErrorMessage(Key));
            }else
            {
               return  ValidationResult.Success;
            }
        }
    }
}
