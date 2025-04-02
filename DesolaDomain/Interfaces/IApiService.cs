using DesolaDomain.Aggregates;

namespace DesolaDomain.Interfaces;

public interface IApiService
{
    Task<string> FetchAccessTokenAsync(string tokenUrl, string clientId, string clientSecret, string providerName);
    Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken);
    Task<T> GetAccessTokenAsync<T>(string cacheKey, string tokenUrl, Dictionary<string, string> parameters, int bufferSeconds = 300);
}