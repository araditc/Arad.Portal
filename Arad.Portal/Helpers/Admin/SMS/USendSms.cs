using System;
using System.IO;
using System.Net;
using Arad.Portal.Models.Admin;
using Newtonsoft.Json;



namespace Arad.Portal.Helpers.Admin.SMS;

public static class USendSms
{

    //private readonly static ILog logger = LogManager.GetLogger(typeof(USendSms));
    public static bool Send_Sms(SMS sms, MessageCenter center)
    {
        try
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(center.AradVasLink1);
            httpWebRequest.ContentType = "application/json; charset=utf-8";
            httpWebRequest.Method = "Post";
            httpWebRequest.Headers.Add("authorization", "Basic " + UBase64.EncodeBase64(center.AradVasUserName + ":" + center.AradVasPassword));
            sms.AradVasLink1 = center.AradVasLink1;
            sms.AradVasNumber = center.AradVasNumber;
            sms.AradVasPassword = center.AradVasPassword;
            sms.AradVasUserName = center.AradVasUserName;
            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                // String s = JsonConvert.SerializeObject(sms);

                streamWriter.Write(JsonConvert.SerializeObject(sms));
                streamWriter.Flush();
            }
            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                string result = streamReader.ReadToEnd();

                ResultSMS rsm = JsonConvert.DeserializeObject<ResultSMS>(result);


                if (rsm.IsSuccessful)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        catch (Exception ex)
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