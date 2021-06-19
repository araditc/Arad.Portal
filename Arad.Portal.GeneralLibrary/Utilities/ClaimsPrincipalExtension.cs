using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.GeneralLibrary.Utilities
{
    public static class ClaimsPrincipalExtension
    {

        public static string GetUserId(this ClaimsPrincipal user)
        {
            string result;
            try
            {
                result = user.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
            catch (Exception)
            {
                result = string.Empty;
            }
            return result;
        }

        public static string GetUserName(this ClaimsPrincipal user)
        {
            string result;
            try
            {
                result = user.FindFirst(ClaimTypes.Name).Value;
            }
            catch (Exception)
            {
                result = string.Empty;
            }
            return result;
        }

        
    }
}
