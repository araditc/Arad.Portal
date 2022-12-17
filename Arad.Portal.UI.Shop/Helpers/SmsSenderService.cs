using Arad.Portal.DataLayer.Contracts.General.Notification;
using Arad.Portal.DataLayer.Entities.General.Notify;
using Arad.Portal.DataLayer.Helpers;
using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.GeneralLibrary.Utilities;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
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
using Serilog;
using AutoMapper.Configuration;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Mime;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Arad.Portal.UI.Shop.Helpers
{
    public class SmsSenderService
    {
        private readonly INotificationRepository _notificationRepository;
        //private readonly Setting _setting;
        private readonly CreateNotification _createNotification;
        private readonly  AppSetting _appSettings;
        private readonly IHttpClientFactory _clientFactory;
        private Timer _timer;
        private bool _flag = true;
        private string _accessToken;
        private static readonly object syncLock = new object();
        public SmsSenderService(INotificationRepository notificationRepository,IHttpClientFactory clientFactory, 
                                /*Setting setting,*/ CreateNotification createNotification)
        {
            _notificationRepository = notificationRepository;
            _clientFactory = clientFactory;
            _createNotification = createNotification;
            _clientFactory = clientFactory;
            _appSettings = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build().Get<AppSetting>();
        }


        public void StartTimer()
        {
            //TimerCallback cb = new(OnTimeEvent);
            _timer = new(OnTimeEvent, null, 1000, 10000);
            Log.Fatal("*************************STTTTTTTttart SMS service timer*************************");
        }
        private async void OnTimeEvent(object state)
        {


            if (_flag)
            {
                await ReadAndSend();
            }
        }

        private async Task ReadAndSend()
        {
            _flag = false;
            UpdateDefinition<Notification> definition;
            Stopwatch sw1 = new();
            Stopwatch sw2 = new();
            int sucessCount = 0;
            int failedCount = 0;
            sw1.Start();

            List<Notification> wholeList = await _notificationRepository.GetForSend(NotificationType.Sms);
            List<Notification> smsNotifications = wholeList.Where(_ => _.ActionType == ActionType.NoExtraAction).ToList();
            if(smsNotifications.Count != 0)
            {
                Log.Fatal($"ReadAndSend smsNotificationCount: {smsNotifications.Count}");
            }
            

            List<Notification> productNotification = wholeList.Where(_ => _.ActionType == ActionType.ProductAvailibilityReminder).ToList();
            if(productNotification.Count != 0)
            {
                Log.Fatal($"ReadAndSend productNotificationCount: {productNotification.Count}");
            }
           

            sw1.Stop();
            sw2.Start();
            if (smsNotifications.Any())
            {
                try
                {

                    var modelToSend = new RahyabRGMessage();
                    modelToSend.Number = _appSettings.SmsEndPointConfig.LineNumber;
                    modelToSend.UserName = _appSettings.SmsEndPointConfig.UserName;
                    modelToSend.Password = _appSettings.SmsEndPointConfig.Password;
                    modelToSend.Company = _appSettings.SmsEndPointConfig.Company;
                    foreach (var notify in smsNotifications)
                    {
                        var obj = new SmsLikeToLikeMessage()
                        {
                            Message = notify.Body,
                            DestNumber = notify.UserPhoneNumber
                        };
                        modelToSend.ListLikeToLikeMessage.Add(obj);
                    }

                    var response = await Send(modelToSend);
                    if(response.StatusCode == HttpStatusCode.OK)
                    {
                        definition = Builders<Notification>.Update.Set(nameof(Notification.SendStatus),
                                NotificationSendStatus.Posted);
                        definition.AddToSet(nameof(Notification.SentDate), DateTime.Now);

                        await _notificationRepository.UpdateMany
                            (smsNotifications.Select(_ => _.NotificationId).ToList(), definition);
                        sucessCount++;
                        Log.Fatal($"Success HttpResponseStatusCode: {response.StatusCode}");
                    }
                    else
                    {
                        definition = Builders<Notification>.Update.Set(nameof(Notification.SendStatus),
                            NotificationSendStatus.Error);
                        definition.AddToSet(nameof(Notification.ScheduleDate), DateTime.Now);
                        await _notificationRepository.UpdateMany
                            (smsNotifications.Select(_ => _.NotificationId).ToList(), definition);
                        failedCount++;
                        Log.Fatal($"Failed HttpResponseStatusCode: {response.StatusCode}");
                    }
                }
                catch (Exception e)
                {
                    definition = Builders<Notification>.Update.Set(nameof(Notification.SendStatus),
                        NotificationSendStatus.Error);
                    definition.AddToSet(nameof(Notification.ScheduleDate), DateTime.Now);
                    await _notificationRepository.UpdateMany
                        (smsNotifications.Select(_ => _.NotificationId).ToList(), definition);
                    failedCount++;
                  
                    Log.Fatal($"Error in sending SMS. Error is: {e.Message}");
                }
            }
            if(productNotification.Any())
            {
                
                foreach (var notify in productNotification)
                {
                    await _createNotification.GenerateProductNotificationToUsers("ProductAvailibilityNotify", notify);
                }
            }

            sw2.Stop();
            if(smsNotifications.Count != 0)
            {
                Log.Fatal($"RowCount: {smsNotifications.Count}\t " +
                              $"ReadTime: {sw1.ElapsedMilliseconds}\t " +
                              $"SendTime: {sw2.ElapsedMilliseconds}\t " +
                              $"SuccessCount: {sucessCount}\t " +
                              $"FailedCount: {failedCount}");
            }
           
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

        private async Task GetToken()
        {
            try
            {
                Log.Fatal("Start Getting token");
                HttpClient client = _clientFactory.CreateClient();
                StringContent content = 
                    new($"{{\"username\":\"{_appSettings.SmsEndPointConfig.TokenUserName}\", \"password\":\"{_appSettings.SmsEndPointConfig.TokenPassword}\"}}",
                    Encoding.UTF8, MediaTypeNames.Application.Json);
                HttpResponseMessage response = await client.PostAsync(_appSettings.SmsEndPointConfig.TokenEndpoint, content);
                
                _accessToken = await response.Content.ReadAsStringAsync();
                
                Log.Fatal($"access token: {_accessToken}");
            }
            catch (Exception e)
            {
                Logger.WriteLogFile($"Error in getting token: {e.Message}");
            }
        }
        private async Task<HttpResponseMessage> Send(RahyabRGMessage data)
        {
            HttpResponseMessage response = null;
            try
            {
                await GetToken();
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
                Log.Fatal($"Json Data for Send: {JsonConvert.SerializeObject(data)}");
                DefaultContractResolver contractResolver = new() { NamingStrategy = new CamelCaseNamingStrategy() };
                StringContent content = new(JsonConvert.SerializeObject(data, 
                    new JsonSerializerSettings { ContractResolver = contractResolver, Formatting = Formatting.Indented }), Encoding.UTF8, MediaTypeNames.Application.Json);
                response = await client.PostAsync(_appSettings.SmsEndPointConfig.Endpoint, content);
                StringContent responseObj = new(JsonConvert.SerializeObject(response, 
                    new JsonSerializerSettings { ContractResolver = contractResolver, Formatting = Formatting.Indented }), Encoding.UTF8, MediaTypeNames.Application.Json);
                Log.Fatal($"whole object after PostAsync : {responseObj}");
                
            }
            catch (Exception e)
            {
                Log.Fatal($"Error in send method: {e.Message}");
            }
            return response;
        }
        public string[] SendSMS_LikeToLike(string[] Message, string[] DestinationAddress,
           string Number, string userName, string password, string IP_Send, string Company)
        {
            string[] RetValue = new string[2];
            RetValue[0] = "False";
            RetValue[1] = "0";
            string Identity = string.Empty;

            if (Message.Length != DestinationAddress.Length)
            {
                RetValue[1] = "Incorrect array size for Messages and Destinations";
                return RetValue;
            }

            int smsSize = DestinationAddress.Length;
            lock (syncLock)
            {
                try
                {
                    Random _Random = new Random(Guid.NewGuid().GetHashCode());
                    Identity = string.Format("{0:yyyyMMddHHmmssfff}", DateTime.Now) + string.Format("{0:000}", _Random.Next(1000));
                    StringBuilder _StringBuilder = new StringBuilder();
                    _StringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    _StringBuilder.Append(Environment.NewLine);
                    _StringBuilder.Append("<!DOCTYPE smsBatch PUBLIC \"-//PERVASIVE//DTD CPAS 1.0//EN\" \"http://www.ubicomp.ir/dtd/Cpas.dtd\">");
                    _StringBuilder.Append(Environment.NewLine);
                    _StringBuilder.Append("<smsBatch company=\"" + Company + "\" batchID=\"" + Company + "+" + Identity + "\">");
                    _StringBuilder.Append(Environment.NewLine);

                    for (int i = 0; i < smsSize; i++)
                    {
                        string strMessage = Message[i];
                        string strDestinationAddress = DestinationAddress[i];

                        string strIsPersian;
                        bool IsPersian = FindTxtLanguage(strMessage);
                        if (IsPersian)
                        {
                            strMessage = C2Unicode(strMessage);
                            strIsPersian = "true";
                        }
                        else
                            strIsPersian = "false";
                        string dcs = IsPersian ? "8" : "0";

                        _StringBuilder.Append("<sms binary=\"" + strIsPersian + "\" dcs=\"" + dcs + "\"" + ">");
                        _StringBuilder.Append(Environment.NewLine);
                        _StringBuilder.Append("<origAddr><![CDATA[" + Validate_Number(Number) + "]]></origAddr>");
                        _StringBuilder.Append(Environment.NewLine);
                        _StringBuilder.Append("<destAddr><![CDATA[" + Validate_Number(strDestinationAddress) + "]]></destAddr>");
                        _StringBuilder.Append(Environment.NewLine);
                        _StringBuilder.Append("<message><![CDATA[" + strMessage + "]]></message>");
                        _StringBuilder.Append(Environment.NewLine);
                        _StringBuilder.Append("</sms>");
                    }

                    _StringBuilder.Append(Environment.NewLine);
                    _StringBuilder.Append("</smsBatch>");

                    string dataToPost = _StringBuilder.ToString();
                    byte[] buf = UTF8Encoding.UTF8.GetBytes(_StringBuilder.ToString());
                    WebRequest objWebRequest = WebRequest.Create(IP_Send);
                    objWebRequest.Method = "POST";
                    objWebRequest.ContentType = "text/xml";
                    byte[] byt = Encoding.UTF8.GetBytes(userName + ":" + password);
                    objWebRequest.Headers.Add("authorization", "Basic " + Convert.ToBase64String(byt));
                    Stream _Stream = objWebRequest.GetRequestStream();
                    StreamWriter _StreamWriter = new StreamWriter(_Stream);
                    _StreamWriter.Write(dataToPost);
                    _StreamWriter.Flush();
                    _StreamWriter.Close();
                    _Stream.Close();

                    WebResponse objWebResponse = objWebRequest.GetResponse();
                    Stream objResponseStream = objWebResponse.GetResponseStream();
                    StreamReader objStreamReader = new StreamReader(objResponseStream);
                    string dataToReceive = objStreamReader.ReadToEnd();
                    objStreamReader.Close();
                    objResponseStream.Close();
                    objWebResponse.Close();

                    if (dataToReceive.IndexOf("CHECK_OK") > 0)
                    {
                        RetValue[0] = "CHECK_OK";
                        RetValue[1] = Identity;
                    }
                    else
                    {
                        try
                        {
                            string msg;
                            int firstIndex = dataToReceive.IndexOf("CDATA[");
                            int LastIndex = dataToReceive.IndexOf("]");
                            msg = dataToReceive.Substring(firstIndex, LastIndex - firstIndex);
                            RetValue[1] = msg;
                            return RetValue;
                        }
                        catch (Exception ex)
                        {
                            RetValue[1] = ex.Message;
                            return RetValue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    RetValue[1] = ex.Message;
                    return RetValue;
                }
            }
            return RetValue;
        }
        public string[] SendSmsSingle(string message, string destinationAddress, string number,
            string userName, string password, string ipSend, string company, bool isFlash)
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
