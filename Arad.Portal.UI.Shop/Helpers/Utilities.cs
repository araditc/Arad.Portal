using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
