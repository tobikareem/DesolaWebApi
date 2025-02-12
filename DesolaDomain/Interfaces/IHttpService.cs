namespace DesolaDomain.Interfaces;

public interface IHttpService
{
    Task<string> PostAsync(string url, IDictionary<string, string> headers, HttpContent content, bool isToken = false);

    Task<string> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken);
}