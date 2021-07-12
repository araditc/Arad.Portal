using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Arad.Portal.DataLayer.Models.User;
using PhoneNumbers;

namespace Arad.Portal.DataLayer.Models.Validator
{
    public class MobilePhoneUserEditValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {

            try
            {
                UserEdit user = (UserEdit)validationContext.ObjectInstance;
                if (string.IsNullOrEmpty(user.PhoneNumber))
                {
                    return new ValidationResult("لطفا تلفن همراه را وارد کنید.");
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
                    return new ValidationResult("شماره همراه وارد شده معتبر نیست.");
                }


                if (isMobile)
                {

                    return ValidationResult.Success;

                }
                else
                {
                    return new ValidationResult(" شماره وارد شده مربوط به تلفن ثابت می باشد.");
                }
            }
#pragma warning disable CS0168 // The variable 'e' is declared but never used
            catch (Exception e)
#pragma warning restore CS0168 // The variable 'e' is declared but never used
            {
                return new ValidationResult(" شماره همراه وارد شده معتبر نیست.");
            }
        }
    }
}
