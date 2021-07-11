using System;
using System.Text;

namespace Arad.Portal.UI.Shop.Dashboard.Helpers.SMS
{
    public class UBase64
    {
        public static string EncodeBase64(string data)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        }
        public static string DecodeBase64(string encode)
        {
            byte[] data = Convert.FromBase64String(encode);
            return Encoding.UTF8.GetString(data);
        }
    }
}