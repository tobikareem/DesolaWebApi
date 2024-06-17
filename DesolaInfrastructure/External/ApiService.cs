using amadeus;
using DesolaDomain.Entities.Authorization;
using DesolaDomain.Enums;
using DesolaDomain.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DesolaInfrastructure.External;

public class ApiService : IApiService
{
    private readonly IHttpService _httpService;
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;
    private readonly Amadeus _amadeus;

    public ApiService(IHttpService httpService, IConfiguration configuration, ICacheService cacheService, Amadeus amadeus)
    {
        _httpService = httpService;
        _configuration = configuration;
        _cacheService = cacheService;
        _amadeus = amadeus;
    }
    public async Task<string> FetchAccessTokenAsync()
    {
        var tokenData = _cacheService.GetItem<TokenAccess>(CacheEntry.AccessToken);

        if (tokenData != null && !tokenData.NeedsRefresh())
        {
            return tokenData.BearerToken ?? string.Empty;
        }

        var url = _configuration["Amadeus_token_endpointUrl"];
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _configuration["Amadeus_client_id"] ?? throw new ArgumentNullException(nameof(_configuration),"unable to find client id")),
            new KeyValuePair<string, string>("client_secret", _configuration["Amadeus_client_secret"] ?? throw new ArgumentNullException(nameof(_configuration),"unable to find client secret") ),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var headers = new Dictionary<string, string>();


        var jsonContent = await _httpService.PostAsync(url, headers, content);
        tokenData = System.Text.Json.JsonSerializer.Deserialize<TokenAccess>(jsonContent) ?? throw new InvalidOperationException("Failed to deserialize token data");

        if (string.IsNullOrWhiteSpace(tokenData.AccessToken))
        {
            throw new InvalidOperationException("Failed to get access token");
        }

        _cacheService.Add(CacheEntry.AccessToken, tokenData, TimeSpan.FromMilliseconds(tokenData.ExpiresIn - 300)); // 300 seconds buffer

        tokenData.RefreshToken(tokenData.AccessToken, tokenData.ExpiresIn);
        return tokenData.BearerToken;

    }

    public async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await _httpService.SendAsync(request, cancellationToken);
        return JsonConvert.DeserializeObject<T>(response) ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}