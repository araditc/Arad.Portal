//
//  --------------------------------------------------------------------
//  Copyright (c) 2005-2021 Arad ITC.
//
//  Author : Ammar Heidari <ammar@arad-itc.org>
//  Licensed under the Apache License, Version 2.0 (the "License")
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0 
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  --------------------------------------------------------------------

using Arad.Portal.DataLayer.Models.Shared;

namespace Arad.Portal.UI.Shop.Helpers
{
    public class OtpHelper
    {
        public static OTP Process(OTP otp)
        {
            return Startup.OTP.AddOrUpdate(otp.Mobile,
                                           otp,
                                           (k, v) =>
                                           {
                                               if ((otp.ExpirationDate - v.ExpirationDate).TotalMinutes < 3)
                                               {
                                                   otp = new () { ExpirationDate = v.ExpirationDate, Mobile = v.Mobile, IsSent = true, Code = v.Code };

                                                   return otp;
                                               }

                                               return otp;
                                           });
        }

        public static OTP Get(string mobile)
        {
            return Startup.OTP.ContainsKey(mobile) ? Startup.OTP[mobile] : null;
        }

        public static OTP Remove(string mobile)
        {
            Startup.OTP.TryRemove(mobile, out OTP value);

            return value;
        }
    }
}
