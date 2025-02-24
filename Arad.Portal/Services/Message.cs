using Arad.SMS.Core.DotNetSDK.Models;
using System.Net.Mime;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Flurl;
using Arad.Portal.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

namespace Arad.Portal.Services;

public class Message : IMessage
{
    private readonly string _apiBaseAddress;
    private readonly HttpClient _httpClient = new();

    public Message(string apiBaseAddress)
    {
        _apiBaseAddress = apiBaseAddress;
        _httpClient.BaseAddress = new(_apiBaseAddress);
    }

    public async Task<string> GetToken(string username, string password, string scope, CancellationToken cancellationToken)
    {
        try
        {
            List<KeyValuePair<string, string>> keyValues = [new("username", username), new("password", password), new("scope", scope)];
            FormUrlEncodedContent formUrlEncodedContent = new(keyValues);
            HttpResponseMessage responseMessage = await _httpClient.PostAsync("/connect/token", formUrlEncodedContent, cancellationToken);

            TokenResponseModel data = JsonConvert.DeserializeObject<TokenResponseModel>(await responseMessage.Content.ReadAsStringAsync(cancellationToken));

            return responseMessage.StatusCode == HttpStatusCode.OK ? data?.access_token : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<HttpResponseMessage> Send(List<AradA2PMessage> aradA2PMessages, string token, bool returnLongId, CancellationToken cancellationToken)
    {
        return await GetResult(JsonConvert.SerializeObject(aradA2PMessages), $"api/message/send/{(returnLongId ? "?returnLongId=true" : "")}", token, cancellationToken);
    }

    public async Task<HttpResponseMessage> SendBulk(AradBulkMessage aradBulkMessage, string token, bool returnLongId, CancellationToken cancellationToken)
    {
        return await GetResult(JsonConvert.SerializeObject(aradBulkMessage), $"api/message/bulk/{(returnLongId ? "?returnLongId=true" : "")}", token, cancellationToken);
    }

    public async Task<List<DLRStatus>> GetDLR(List<string> ids, string token, bool returnLongId, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await GetResult(JsonConvert.SerializeObject(ids), $"api/message/GetDLR/{(returnLongId ? "?returnLongId=true" : "")}", token, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            ResultApiClass<List<DLRStatus>> resultApi = JsonConvert.DeserializeObject<ResultApiClass<List<DLRStatus>>>(await response.Content.ReadAsStringAsync(cancellationToken));

            return resultApi.Data;
        }

        return [];
    }

    public async Task<List<Inbox>> GetMO(bool returnId, string token, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await GetResult($"api/message/GetMO/{(returnId ? "?returnId=true" : "")}", token, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            ResultApiClass<List<Inbox>> resultApi = JsonConvert.DeserializeObject<ResultApiClass<List<Inbox>>>(await response.Content.ReadAsStringAsync(cancellationToken));

            return resultApi.Data;
        }

        return [];
    }

    public async Task<List<Inbox>> GetMOByDate(DateTime startDateTime, DateTime endDateTime, bool returnId, string token, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await GetResult($"api/message/GetMOByDate?startDateTime={startDateTime}&endDateTime={endDateTime}{(returnId ? "&returnId=true" : "")}", token, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            ResultApiClass<List<Inbox>> resultApi = JsonConvert.DeserializeObject<ResultApiClass<List<Inbox>>>(await response.Content.ReadAsStringAsync(cancellationToken));

            return resultApi.Data;
        }

        return [];
    }

    private async Task<HttpResponseMessage> GetResult(string content, string url, string token, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        StringContent stringContent = new(content, Encoding.UTF8, MediaTypeNames.Application.Json);

        return await _httpClient.PostAsync(Url.Combine(_apiBaseAddress, url), stringContent, cancellationToken);
    }

    private async Task<HttpResponseMessage> GetResult(string url, string token, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        return await _httpClient.GetAsync(Url.Combine(_apiBaseAddress, url), cancellationToken);
    }
}