using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Arad.Portal.DataLayer.Models.User;
using PhoneNumbers;

namespace Arad.Portal.DataLayer.Models.Validator
{
    public class MobilePhoneUserDTOValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {

            try
            {
                UserDTO user = (UserDTO)validationContext.ObjectInstance;
                if (string.IsNullOrEmpty(user.PhoneNumber))
                {
                    return new ValidationResult("AlertAndMessage_PhoneNumberRequired");
                }

                PhoneNumberUtil phoneUtil = PhoneNumberUtil.GetInstance();

                PhoneNumber phoneNumber = phoneUtil.Parse(user.FullMobile, null);

                bool isMobile = false;
                bool isValidNumber = phoneUtil.IsValidNumber(phoneNumber);
                var numberType = phoneUtil.GetNumberType(phoneNumber); // Produces Mobile , FIXED_LINE 
                string phoneNumberType = numberType.ToString();

                if (!string.IsNullOrEmpty(phoneNumberType) && phoneNumberType == "MOBILE")
                {
                    isMobile = true;
                }

                if (!isValidNumber)
                {
                    return new ValidationResult("Validation_MobileNumberInvalid1");
                }
                if (isMobile)
                {

                    return ValidationResult.Success;

                }
                else
                {
                    return new ValidationResult("Validation_MobileNumberInvalid2");
                }
            }
            catch (Exception e)
            {
                return new ValidationResult("Validation_CellPhoneNumber");
            }
        }
    }
}
