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
    }
}
