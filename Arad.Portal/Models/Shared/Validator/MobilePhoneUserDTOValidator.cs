using System;
using System.ComponentModel.DataAnnotations;
using Arad.Portal.Models.Shared.User;
using PhoneNumbers;

namespace Arad.Portal.Models.Shared.Validator;

public class MobilePhoneUserDtoValidator : ValidationAttribute
{
    protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
    {

        try
        {
            UserDto user = (UserDto)validationContext.ObjectInstance;
            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                return new("AlertAndMessage_PhoneNumberRequired");
            }

            PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

            PhoneNumber phoneNumber = phoneUtil.Parse(user.FullMobile, null);

            bool isMobile = false;
            bool isValidNumber = phoneUtil.IsValidNumber(phoneNumber);
            PhoneNumberType numberType = phoneUtil.GetNumberType(phoneNumber); // Produces Mobile , FIXED_LINE 
            string phoneNumberType = numberType.ToString();

            if (!string.IsNullOrEmpty(phoneNumberType) && phoneNumberType == "MOBILE")
            {
                isMobile = true;
            }

            if (!isValidNumber)
            {
                return new("Validation_MobileNumberInvalid1");
            }
            if (isMobile)
            {

                return ValidationResult.Success;

            }
            else
            {
                return new("Validation_MobileNumberInvalid2");
            }
        }
        catch (Exception e)
        {
            return new("Validation_CellPhoneNumber");
        }
    }
}