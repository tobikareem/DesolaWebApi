using DesolaDomain.Aggregates;

namespace DesolaDomain.Interfaces;

public interface IApiService
{
    Task<string> FetchAccessTokenAsync();
    Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken);
    Task<T> GetAccessTokenAsync<T>(string cacheKey, string tokenUrl, Dictionary<string, string> parameters, int bufferSeconds = 300);
}