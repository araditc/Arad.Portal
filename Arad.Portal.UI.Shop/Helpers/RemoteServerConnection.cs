
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Helpers
{
    public class RemoteServerConnection
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        public RemoteServerConnection(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }
        public async Task<string> GetToken()
        {
            try
            {
                var tokenUrl = _configuration["StaticFilesPlace:TokenUrl"];
                var userName = _configuration["StaticFilesPlace:User"];
                var password = _configuration["StaticFilesPlace:Password"];
                var scope = _configuration["StaticFilesPlace:Scope"];

                var client = _clientFactory.CreateClient();
                var keyValues = new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string>("username", userName),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("scope", /*"ApiAccess"*/scope),
                };

                client.BaseAddress = new Uri(tokenUrl);
                var content = new FormUrlEncodedContent(keyValues);
                var response = await client.PostAsync(new Uri(tokenUrl), content).Result.Content.ReadAsStringAsync();
                //var response = await client.PostAsync(/*"/connect/token",*/ content).Content.ReadAsStringAsync();
                return response;
                //T
                //var data = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponseModel>(await response.Content.ReadAsStringAsync());
                //access_token = data.access_token;
                //Logger.WriteLogFile($"access token = {access_token}");

            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        //private  async Task RelayViaHttpGetAsync<T>(string host, string query, string messageId,
        //   T model, int tryCount, string auth) where T :class
        //{

        //    //FullLogOption.OptionalLog("In Relay via get.");
        //    //FullLogOption.OptionalLog($"{host} {query} {JsonConvert.SerializeObject(model)}");

        //    //FullLogOption.OptionalLog($"relay via get {host} {query} {JsonConvert.SerializeObject(model)}");

        //    var st = new Stopwatch();
        //    st.Start();

        //    using var serviceScope = _hostBuild.Services.CreateScope();
        //    {
        //        var services = serviceScope.ServiceProvider;
        //        //_clientFactory = services.GetService<IHttpClientFactory>();
        //        var client = _clientFactory.CreateClient();

        //        if (!string.IsNullOrWhiteSpace(auth))
        //        {
        //            client.DefaultRequestHeaders.Add("Authorization", "Basic " + auth);
        //        }

        //        client.Timeout = TimeSpan.FromMilliseconds(300);
        //        var address = Flurl.Url.Combine(host, query);


        //        var response = await client.GetAsync(address);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            throw new Exception();
        //            throw new ApiDeliveryEngineRetryException { CurrentRetryCount = tryCount, Model = model, TimeElapsed = st.Elapsed, Method = "Get", StatusCode = response.StatusCode };
        //        }

        //        Logger.WriteLogFile($"Get time: {st.ElapsedMilliseconds}");
        //    }
        //}

        //private static async Task RelayViaHttpPostAsync(ApiDeliveryQueueDto model, string host,
        //    List<RelayParams> additionalParams, int tryCount, string auth)
        //{
        //    FullLogOption.OptionalLog($"relay via post {host} {JsonConvert.SerializeObject(additionalParams)} {JsonConvert.SerializeObject(model)}");

        //    var st = new Stopwatch();
        //    st.Start();

        //    using var serviceScope = _hostBuild.Services.CreateScope();
        //    {
        //        var services = serviceScope.ServiceProvider;
        //        _clientFactory = services.GetService<IHttpClientFactory>();

        //        var client = _clientFactory.CreateClient();
        //        if (!string.IsNullOrWhiteSpace(auth))
        //        {
        //            client.DefaultRequestHeaders.Add("Authorization", "Basic " + auth);
        //        }

        //        var content = new StringContent(
        //            JsonConvert.SerializeObject(new
        //            {
        //                BatchId = model.Object.MessageId,
        //                model.Object.Status,
        //                model.Object.PartNumber,
        //                model.Object.Mobile,
        //                ExtraParameters = additionalParams
        //            }),
        //            Encoding.UTF8, MediaTypeNames.Application.Json);
        //        var response = await client.PostAsync(host, content);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            throw new ApiDeliveryEngineRetryException { CurrentRetryCount = tryCount, Model = model, Address = host, TimeElapsed = st.Elapsed, Method = "Post", StatusCode = response.StatusCode };
        //        }

        //        Logger.WriteLogFile($"Post time: {st.ElapsedMilliseconds}");
        //    }
        //}
    }
}
