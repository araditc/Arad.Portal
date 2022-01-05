using Microsoft.AspNetCore.Identity;
using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers
{
    public class Utilities
    {
        public static string GenerateOtp()
        {
            Random generator = new();
            string generateOtp = generator.Next(0, 999999).ToString("D6");

            return generateOtp;
        }

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
            IdentityModel.CryptoRandom rand = new();
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

    }
        
}

