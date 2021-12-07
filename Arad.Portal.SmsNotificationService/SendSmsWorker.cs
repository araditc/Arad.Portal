using Arad.Portal.DataLayer.Contracts.General.Notification;
using Arad.Portal.DataLayer.Entities.General.Notify;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Arad.Portal.DataLayer.Models.Shared.Enums;

namespace Arad.Portal.SmsNotificationService
{
    public class SendSmsWorker : BackgroundService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly Setting _setting;
        private Timer _timer;
        private bool _flag = true;

        public SendSmsWorker(INotificationRepository notificationRepository, Setting setting)
        {
            _notificationRepository = notificationRepository;
            _setting = setting;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.WriteLogFile($"Service started at: {DateTime.Now}");
            _timer = new(OnTimeEvent, null, 1000, 1000);
        }

        private async void OnTimeEvent(object state)
        {
            if (Logger.LogFlag)
            {
                Logger.WriteLogFile(_setting.ServiceName, _setting.LogPath);
            }

            if (_flag)
            {
                await ReadAndSend();
            }
        }

        private async Task ReadAndSend()
        {
            _flag = false;
            Stopwatch sw1 = new();
            Stopwatch sw2 = new();
            int sucessCount = 0;
            int failedCount = 0;
            sw1.Start();
            List<Notification> notifications = await _notificationRepository.GetForSend(NotificationType.Sms);
            sw1.Stop();
            if (notifications.Any())
            {
                sw2.Start();
                foreach (Notification notification in notifications)
                {
                    try
                    {
                        string[] resultSms = SendSmsSingle(notification.Body,
                                                           notification.UserPhoneNumber,
                                                           notification.SendSmsConfig.AradVas_Number,
                                                           notification.SendSmsConfig.AradVas_UserName,
                                                           notification.SendSmsConfig.AradVas_Password,
                                                           notification.SendSmsConfig.AradVas_Link_1,
                                                           notification.SendSmsConfig.AradVas_Domain,
                                                           false);

                        //HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(notification.SendSmsConfig.AradVas_Link_1);
                        //httpWebRequest.ContentType = "application/json; charset=utf-8";
                        //httpWebRequest.Method = "Post";
                        //httpWebRequest.Headers.Add("authorization", "Basic " + Base64Encode(notification.SendSmsConfig.AradVas_UserName + ":" + notification.SendSmsConfig.AradVas_Password));
                        //Sms sms = new()
                        //{
                        //    SmsText = notification.Body,
                        //    Receivers = notification.UserPhoneNumber,
                        //    AradVas_Link_1 = notification.SendSmsConfig.AradVas_Link_1,
                        //    AradVas_Number = notification.SendSmsConfig.AradVas_Number,
                        //    AradVas_Password = notification.SendSmsConfig.AradVas_Password,
                        //    AradVas_UserName = notification.SendSmsConfig.AradVas_UserName
                        //};
                        //await using (StreamWriter streamWriter = new(httpWebRequest.GetRequestStream()))
                        //{
                        //    await streamWriter.WriteAsync(JsonConvert.SerializeObject(sms));
                        //    await streamWriter.FlushAsync();
                        //}

                        //HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                        //using StreamReader streamReader = new(httpResponse.GetResponseStream());

                        //string result = await streamReader.ReadToEndAsync();

                        //ResultSms resultSms = JsonConvert.DeserializeObject<ResultSms>(result);

                        if (resultSms[0].Equals("CHECK_OK"))
                        {
                            notification.SendStatus = NotificationSendStatus.Posted;
                            notification.SentDate = DateTime.Now;
                            await _notificationRepository.Update(notification, resultSms[1], true);
                            sucessCount++;
                        }
                        else
                        {
                            notification.SendStatus = NotificationSendStatus.Error;
                            notification.ScheduleDate = DateTime.Now.AddMinutes(5);
                            await _notificationRepository.Update(notification, resultSms[1], true);
                            failedCount++;
                        }
                    }
                    catch (Exception e)
                    {
                        failedCount++;
                        notification.SendStatus = NotificationSendStatus.Error;
                        notification.ScheduleDate = DateTime.Now.AddMinutes(5);
                        await _notificationRepository.Update(notification, e.Message, true);
                        Logger.WriteLogFile($"Error in sending SMS. Error is: {e.Message}");
                    }
                }
                sw2.Stop();
                Logger.WriteLogFile($"RowCount: " +
                    $"{notifications.Count}\t " +
                    $"ReadTime: {sw1.ElapsedMilliseconds}\t " +
                    $"SendTime: {sw2.ElapsedMilliseconds}\t " +
                    $"SuccessCount: {sucessCount}\t " +
                    $"FailedCount: {failedCount}");
            }

            //string Base64Encode(string plainText)
            //{
            //    byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            //    return Convert.ToBase64String(plainTextBytes);
            //}

            _flag = true;
        }

        public static bool FindTxtLanguage(string unicodeString)
        {
            const int maxAnsiCode = 255;
            bool isPersian = unicodeString == string.Empty || unicodeString.ToCharArray().Any(c => c > maxAnsiCode);
            return isPersian;
        }

        private static readonly object SyncLock = new();

        public string Validate_Number(string number)
        {
            string ret = number.Trim();
            if (ret.Substring(0, 4) == "0098")
            {
                ret = ret.Remove(0, 4);
            }
            if (ret.Substring(0, 3) == "098")
            {
                ret = ret.Remove(0, 3);
            }
            if (ret.Substring(0, 3) == "+98")
            {
                ret = ret.Remove(0, 3);
            }
            if (ret.Substring(0, 2) == "98")
            {
                ret = ret.Remove(0, 2);
            }
            if (ret.Substring(0, 1) == "0")
            {
                ret = ret.Remove(0, 1);
            }
            return "+98" + ret;
        }

        public string ValidateMessage(ref string message, bool isPersian)
        {
            char cr = (char)13;
            message = message.Replace(cr.ToString(), string.Empty);

            if (message.EndsWith(Environment.NewLine))
            {
                message = message.TrimEnd(Environment.NewLine.ToCharArray());
            }
            return isPersian ? C2Unicode(message) : message;
        }

        public string C2Unicode(string message)
        {
            int i;
            string ret = "";

            for (i = 0; i < message.Length; i++)
            {
                int preUnicodeNumber = 4 - $"{(int)(message.Substring(i, 1)[0]):X}".Length;
                string preUnicode = string.Format("{0:D" + preUnicodeNumber + "}", 0);
                string strHex = preUnicode + $"{(int)message.Substring(i, 1)[0]:X}";
                if (strHex.Length == 4)
                {
                    ret += strHex;
                }
            }
            return ret;
        }

        public string[] SendSmsSingle(string message, string destinationAddress, string number, string userName, string password, string ipSend, string company, bool isFlash)
        {
            string strIsPersian;
            string[] retValue = new string[2];
            retValue[0] = "False";
            retValue[1] = "0";
            bool isPersian = FindTxtLanguage(message);
            ValidateMessage(ref message, isPersian);
            if (isPersian)
            {
                message = C2Unicode(message);
                strIsPersian = "true";
            }
            else
            {
                strIsPersian = "false";
            }

            lock (SyncLock)
            {
                try
                {
                    Random random = new();
                    string identity = $"{DateTime.Now:yyyyMMddHHmmssfff}{random.Next(1000):000}";
                    string dcs = isPersian ? "8" : "0";
                    string msgClass = isFlash ? "0" : "1";
                    StringBuilder stringBuilder = new();
                    stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("<!DOCTYPE smsBatch PUBLIC \"-//PERVASIVE//DTD CPAS 1.0//EN\" \"http://www.ubicomp.ir/dtd/Cpas.dtd\">");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("<smsBatch company=\"" + company + "\" batchID=\"" + company + "+" + identity + "\">");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("<sms msgClass=\"" + msgClass + "\" binary=\"" + strIsPersian + "\" dcs=\"" + dcs + "\"" + ">");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("<destAddr><![CDATA[" + Validate_Number(destinationAddress) + "]]></destAddr>");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("<origAddr><![CDATA[" + Validate_Number(number) + "]]></origAddr>");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("<message><![CDATA[" + message + "]]></message>");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("</sms>");
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append("</smsBatch>");

                    string dataToPost = stringBuilder.ToString();
                    WebRequest objWebRequest = WebRequest.Create(ipSend);
                    objWebRequest.Method = "POST";
                    objWebRequest.ContentType = "text/xml";
                    byte[] byt = Encoding.UTF8.GetBytes(userName + ":" + password);
                    objWebRequest.Headers.Add("authorization", "Basic " + Convert.ToBase64String(byt));
                    Stream stream = objWebRequest.GetRequestStream();
                    StreamWriter streamWriter = new(stream);
                    streamWriter.Write(dataToPost);
                    streamWriter.Flush();
                    streamWriter.Close();
                    stream.Close();

                    WebResponse objWebResponse = objWebRequest.GetResponse();
                    Stream objResponseStream = objWebResponse.GetResponseStream();

                    if (objResponseStream != null)
                    {
                        StreamReader objStreamReader = new(objResponseStream);
                        string dataToReceive = objStreamReader.ReadToEnd();
                        objStreamReader.Close();
                        objResponseStream.Close();
                        objWebResponse.Close();

                        if (dataToReceive.Contains("CHECK_OK"))
                        {
                            retValue[0] = "CHECK_OK";
                            retValue[1] = identity;
                            string[] tonumber = new string[1];
                            tonumber[0] = destinationAddress;
                        }
                        else
                        {
                            try
                            {
                                int firstIndex = dataToReceive.IndexOf("CDATA[", StringComparison.Ordinal);
                                int lastIndex = dataToReceive.IndexOf("]", StringComparison.Ordinal);
                                string msg = dataToReceive.Substring(firstIndex + 6, lastIndex - firstIndex - 6);
                                retValue[1] = msg;
                                return retValue;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return retValue;
                }
                return retValue;
            }
        }
    }
}
