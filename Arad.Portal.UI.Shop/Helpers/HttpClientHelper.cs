using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Arad.Portal.UI.Shop.Helpers
{
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
}
