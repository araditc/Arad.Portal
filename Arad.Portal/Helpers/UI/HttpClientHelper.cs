using System.Net.Http;

namespace Arad.Portal.Helpers.UI;

public class HttpClientHelper
{
    private readonly HttpClient _clientFactory;


    public HttpClientHelper(IHttpClientFactory clientHelper)
    {
        _clientFactory = clientHelper.CreateClient();
    }

    public HttpClient GetClient()
    {
        return _clientFactory;
    }
}