using Arad.Portal.GeneralLibrary.Utilities;

using System.ComponentModel.DataAnnotations;

namespace Arad.Portal.GeneralLibrary.CustomAttributes;

public class ErrorMessage : ValidationAttribute
{
    public ErrorMessage(string key)
    {
        Key = key;
    }

    public string Key { get; }

    public string GetErrorMessage()
    {
        return UtilityLanguage.GetString(Key);
    }

    protected override ValidationResult IsValid(object value,
                                                ValidationContext validationContext)
    {
        return value == null ? new ValidationResult(GetErrorMessage()) :
                   ValidationResult.Success;
    }
}