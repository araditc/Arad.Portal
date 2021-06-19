using Arad.Portal.GeneralLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.GeneralLibrary.CustomAttributes
{
    public class CustomErrorMessageAttribute : ValidationAttribute
    {
        public CustomErrorMessageAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; }

        public string GetErrorMessage()
        {
            return Language.GetString(Key);
        }

        protected override ValidationResult IsValid(object value,
            ValidationContext validationContext)
        {
            return value == null ? new ValidationResult(GetErrorMessage()) :
                ValidationResult.Success;
        }
    }
}
