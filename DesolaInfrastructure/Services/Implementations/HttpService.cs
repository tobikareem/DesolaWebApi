
using DesolaDomain.Interfaces;
using System.Net.Http;
using System.Text.Json;

namespace DesolaInfrastructure.Services.Implementations
{
    public class HttpService : IHttpService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        public async Task<string> PostAsync(string url, IDictionary<string, string> headers, HttpContent content, bool isToken = false)
        {
            var client = _httpClientFactory.CreateClient();
            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SendAsync(HttpRequestMessage request)
        {

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
