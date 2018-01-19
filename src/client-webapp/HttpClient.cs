using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace client_webapp
{
	public class HttpClient
	{
		private readonly System.Net.Http.HttpClient _httpClient;
		private readonly ILogger<HttpClient> _logger;
		public static readonly Func<Uri, string> GetError = uri => $"Error requesting url: {uri}";

		public static readonly Func<Uri, HttpResponseMessage, string> GetErrorOnReceivingResponse = (uri, response) =>
			$"{GetError(uri)}. Returned status code: {response.StatusCode} Reason: {response.ReasonPhrase}";

		public HttpClient(System.Net.Http.HttpClient httpClient, ILogger<HttpClient> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		public async Task<string> GetStringAsync(Uri url)
		{
			HttpResponseMessage response = await Invoke(url); 
			return response.IsSuccessStatusCode
					? await response.Content.ReadAsStringAsync()
					: throw new HttpException(url, response);
		}

		public async Task<T> GetAsync<T>(Uri url)
		{
			HttpResponseMessage response = await Invoke(url);

			return response.IsSuccessStatusCode
				? JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync())
				: throw new HttpException(url, response);
		}

		private async Task<HttpResponseMessage> Invoke(Uri url)
		{
			HttpResponseMessage response;
			try
			{
				response = await _httpClient.GetAsync(url);
				_logger.LogInformation($"Requesting url {url}");
			}
			catch (Exception ex)
			{
				_logger.LogError(new EventId(0), ex, GetError(url));
				throw new HttpException(url, ex);
			}
			return response;
		}

		public HttpClient AuthorizationHeader(string scheme, string value)
		{
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, value);
			return this;
		}
	}

	public class HttpException : Exception
	{
		public readonly HttpStatusCode StatusCode;

		public HttpException(Uri uri, HttpResponseMessage response) : 
			base(HttpClient.GetErrorOnReceivingResponse(uri, response))
		{
			StatusCode = response.StatusCode;
		}

		public HttpException(Uri uri, Exception exception) : 
			base(HttpClient.GetError(uri), exception)
		{
			StatusCode = HttpStatusCode.InternalServerError;
		}
	}
}