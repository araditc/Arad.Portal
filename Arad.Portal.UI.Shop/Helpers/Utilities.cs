using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Helpers
{
    public class Utilities
    {
        ///// <summary>  
        ///// set the cookie  
        ///// </summary>  
        ///// <param name="key">key (unique indentifier)</param>  
        ///// <param name="value">value to store in cookie object</param>  
        ///// <param name="expireTime">expiration time</param>  
        //public static void SetCookie(string key, string value, int? expireTime)
        //{
        //    CookieOptions option = new CookieOptions();

        //    if (expireTime.HasValue)
        //        option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
        //    else
        //        option.Expires = DateTime.Now.AddMilliseconds(10);

        //    Response.Cookies.Append(key, value, option);
        //}


        /// <summary>
        ///     Generates a Random Password
        ///     respecting the given strength requirements.
        /// </summary>
        /// <param name="opts">
        ///     A valid PasswordOptions object
        ///     containing the password strength requirements.
        /// </param>
        /// <returns>A random password</returns>
        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            opts ??= new() { RequiredLength = 8, RequiredUniqueChars = 4, RequireDigit = true, RequireLowercase = true, RequireNonAlphanumeric = true, RequireUppercase = true };

            string[] randomChars = {
                                       "ABCDEFGHJKLMNPQRSTUVWXYZ", // uppercase 
                                       "abcdefghijkmnpqrstuvwxyz", // lowercase
                                       "123456789" // digits
                                       //"" // non-alphanumeric
                                   };
            CryptoRandom rand = new();
            List<char> chars = new();

            if (opts.RequireUppercase)
            {
                chars.Insert(rand.Next(0, chars.Count), randomChars[0][rand.Next(0, randomChars[0].Length)]);
            }

            if (opts.RequireLowercase)
            {
                chars.Insert(rand.Next(0, chars.Count), randomChars[1][rand.Next(0, randomChars[1].Length)]);
            }

            if (opts.RequireDigit)
            {
                chars.Insert(rand.Next(0, chars.Count), randomChars[2][rand.Next(0, randomChars[2].Length)]);
            }

            //if (opts.RequireNonAlphanumeric)
            //{
            //    chars.Insert(rand.Next(0, chars.Count), randomChars[3][rand.Next(0, randomChars[3].Length)]);
            //}

            for (int i = chars.Count; i < opts.RequiredLength || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count), rcs[rand.Next(0, rcs.Length)]);
            }

            return new(chars.ToArray());
        }

        public static string GenerateOtp()
        {
            Random generator = new();
            string generateOtp = generator.Next(0, 999999).ToString("D6");

            return generateOtp;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static dynamic ConvertDynamic(dynamic source, Type dest)
        {
            return Convert.ChangeType(source, dest);
        }
    }
}
