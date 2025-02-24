using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.Helpers.Shared;

public class OtpHelper
{
    public static OTP Process(OTP otp)
    {
        return Program.OTP.AddOrUpdate(otp.Mobile,
                                       otp,
                                       (k, v) =>
                                       {
                                           if ((otp.ExpirationDate - v.ExpirationDate).TotalMinutes < 3)
                                           {
                                               otp = new() { ExpirationDate = v.ExpirationDate, Mobile = v.Mobile, IsSent = true, Code = v.Code };

                                               return otp;
                                           }

                                           return otp;
                                       });
    }

    public static OTP Get(string mobile)
    {
        return Program.OTP.ContainsKey(mobile) ? Program.OTP[mobile] : null;
    }

    public static OTP Remove(string mobile)
    {
        Program.OTP.TryRemove(mobile, out OTP value);

        return value;
    }
}