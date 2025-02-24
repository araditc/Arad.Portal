using System;
using System.ComponentModel.DataAnnotations;
using Arad.Portal.Models.Shared.User;
using PhoneNumbers;

namespace Arad.Portal.Models.Shared.Validator;

internal class MobilePhoneValidator : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        try
        {
            RegisterUserModel user = (RegisterUserModel)validationContext.ObjectInstance;

            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                return new("AlertAndMessage_PhoneNumberRequired");
            }

            if (string.IsNullOrEmpty(user.FullMobile))
            {
                return new("AlertAndMessage_Error");
            }

            // Get an instance of PhoneNumberUtil
            PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

            // Parse the phone number (ensure FullMobile includes the country code or provide a default country code)
            PhoneNumber phoneNumber;
            try
            {
                phoneNumber = phoneUtil.Parse(user.FullMobile, "IR");  // Replace "IR" with your default country code if needed
            }
            catch (NumberParseException)
            {
                return new("Validation_MobileNumberInvalid1");
            }

            // Check if it's a valid number
            bool isValidNumber = phoneUtil.IsValidNumber(phoneNumber);

            if (!isValidNumber)
            {
                return new("Validation_MobileNumberInvalid1");
            }

            // Check if the number is a mobile number
            PhoneNumberType numberType = phoneUtil.GetNumberType(phoneNumber);
            if (numberType != PhoneNumberType.MOBILE)
            {
                return new("Validation_MobileNumberInvalid2");
            }

            return ValidationResult.Success;  // Validation successful
        }
        catch (Exception ex)
        {
            // Log exception if necessary
            return new("Validation_CellPhoneNumber");
        }
    }
}
