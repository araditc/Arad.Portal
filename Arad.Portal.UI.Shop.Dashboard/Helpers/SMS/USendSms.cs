using System;
using System.IO;
using System.Net;
using Arad.Portal.DataLayer.Models.Shared;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;



namespace Arad.Portal.UI.Shop.Dashboard.Helpers.SMS
{
    public static class USendSms
    {
        
        //private readonly static ILog logger = LogManager.GetLogger(typeof(USendSms));
        public static Boolean Send_Sms(SMS sms, MessageCenter center)
        {
            try
            {
                var HttpWebRequest = (HttpWebRequest)WebRequest.Create(center.AradVas_Link_1);
                HttpWebRequest.ContentType = "application/json; charset=utf-8";
                HttpWebRequest.Method = "Post";
                HttpWebRequest.Headers.Add("authorization", "Basic " + UBase64.EncodeBase64(center.AradVas_UserName + ":" + center.AradVas_Password));
                sms.AradVas_Link_1 = center.AradVas_Link_1;
                sms.AradVas_Number = center.AradVas_Number;
                sms.AradVas_Password = center.AradVas_Password;
                sms.AradVas_UserName = center.AradVas_UserName;
                using (var streamWriter = new StreamWriter(HttpWebRequest.GetRequestStream()))
                {
                    // String s = JsonConvert.SerializeObject(sms);

                    streamWriter.Write(JsonConvert.SerializeObject(sms));
                    streamWriter.Flush();
                }
                var HttpResponse = (HttpWebResponse)HttpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(HttpResponse.GetResponseStream()))
                {
                    var Result = streamReader.ReadToEnd();

                    ResultSMS RSM = JsonConvert.DeserializeObject<ResultSMS>(Result);


                    if (RSM.IsSuccessful)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception EX)
            {
                //logger.Error($"Send_Sms  error 36: {EX.Message}");
                return false;
            }
        }

        //public static Boolean Send_Sms_ViaURL(SaPost.API.Models.SMS sms)
        //{
        //    try
        //    {

        //        var url = AradVas_Link_2 +
        //         "?Username=" + AradVas_UserName +
        //         "&password=" + AradVas_Password +
        //         "&senderId=" + sms.SenderId +
        //         "&SmsText=" + sms.SmsText +
        //         "&Receivers=" + sms.Receivers;
        //        var HttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

        //        System.Net.WebResponse resp = HttpWebRequest.GetResponse();
        //        using (System.IO.Stream stream = resp.GetResponseStream())
        //        {
        //            using (System.IO.StreamReader sr = new System.IO.StreamReader(stream))
        //            {
        //                var Result = sr.ReadToEnd();

        //                sr.Close();
        //                ResultSMS RSM = JsonConvert.DeserializeObject<ResultSMS>(Result);


        //                if (RSM.IsSuccessful)
        //                {
        //                    return true;
        //                }
        //                else
        //                {
        //                    return false;
        //                }
        //            }
        //        }




        //    }
        //    catch (Exception EX)
        //    {

        //        // logger.Error($"Send_Sms  error 36: {EX.Message}");
        //        return false;
        //    }
        //}
    }
}