using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Arad.Portal.Helpers.Shared;

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
            string tokenUrl = _configuration["StaticFilesPlace:TokenUrl"];
            string userName = _configuration["StaticFilesPlace:User"];
            string password = _configuration["StaticFilesPlace:Password"];
            string scope = _configuration["StaticFilesPlace:Scope"];

            HttpClient client = _clientFactory.CreateClient();
            List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>> {
                                                                                                      new("username", userName),
                                                                                                      new("password", password),
                                                                                                      new("scope", /*"ApiAccess"*/scope),
                                                                                                  };

            client.BaseAddress = new(tokenUrl);
            FormUrlEncodedContent content = new FormUrlEncodedContent(keyValues);
            string response = await client.PostAsync(new Uri(tokenUrl), content).Result.Content.ReadAsStringAsync();
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