using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace api_gateway
{
    public class FixHeadersHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("administrator: administrator")));
            var response = await base.SendAsync(request, cancellationToken);
            response.Headers.Remove("Access-Control-Allow-Origin");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            return response;
        }
    }
}