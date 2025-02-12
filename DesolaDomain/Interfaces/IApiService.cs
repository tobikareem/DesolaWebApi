using DesolaDomain.Aggregates;

namespace DesolaDomain.Interfaces;

public interface IApiService
{
    Task<string> FetchAccessTokenAsync();
    Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken);
}