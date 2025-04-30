
using System.Text.Json;
using Desola.Common.Exceptions;
using DesolaDomain.Interfaces;

namespace DesolaInfrastructure.Services.Implementations;

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

    public async Task<string> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.SendAsync(request, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode) return content;

        try
        {
            var amadeusError = JsonSerializer.Deserialize<AmadeusErrorResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            throw new AmadeusApiException(response.StatusCode, amadeusError ?? new AmadeusErrorResponse());
        }
        catch (JsonException)
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {content}");
        }
    }

}