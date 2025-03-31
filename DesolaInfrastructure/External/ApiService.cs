using amadeus;
using DesolaDomain.Entities.Authorization;
using DesolaDomain.Enums;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DesolaInfrastructure.External;

public class ApiService : IApiService
{
    private readonly IHttpService _httpService;
    private readonly AppSettings _appSettings;
    private readonly ICacheService _cacheService;
    private readonly Amadeus _amadeus;

    public ApiService(IHttpService httpService, IOptions<AppSettings> configuration, ICacheService cacheService, Amadeus amadeus)
    {
        _httpService = httpService;
        _appSettings = configuration.Value;
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

        var url = _appSettings.ExternalApi.Amadeus.TokenEndpointUrl;
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _appSettings.ExternalApi.Amadeus.ClientId ?? throw new ArgumentNullException(nameof(_appSettings),"unable to find client id")),
            new KeyValuePair<string, string>("client_secret", _appSettings.ExternalApi.Amadeus.ClientSecret ?? throw new ArgumentNullException(nameof(_appSettings),"unable to find client secret") ),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var headers = new Dictionary<string, string>();


        var jsonContent = await _httpService.PostAsync(url, headers, content);
        tokenData = JsonSerializer.Deserialize<TokenAccess>(jsonContent) ?? throw new InvalidOperationException("Failed to deserialize token data");

        if (string.IsNullOrWhiteSpace(tokenData.AccessToken))
        {
            throw new InvalidOperationException("Failed to get access token");
        }

        _cacheService.Add(CacheEntry.AccessToken, tokenData, TimeSpan.FromMilliseconds(tokenData.ExpiresIn - 300)); // 300 seconds buffer

        tokenData.RefreshToken(tokenData.AccessToken, tokenData.ExpiresIn);
        return tokenData.BearerToken;

    }

    public async Task<T> GetAccessTokenAsync<T>(string cacheKey, string tokenUrl, Dictionary<string, string> parameters, int bufferSeconds = 300)
    {
        var cachedToken = _cacheService.GetItem<T>(cacheKey);
        if (cachedToken != null)
        {
            return cachedToken;
        }
        
        var content = new FormUrlEncodedContent(parameters);
        var response = await _httpService.PostAsync(tokenUrl, new Dictionary<string, string>(), content, true);
        
        var tokenData = JsonSerializer.Deserialize<T>(response) ?? throw new Exception("Invalid token response");
        
        var tokenExpiration = GetTokenExpirationTime(tokenData);
        var cacheDuration = tokenExpiration > 0 ? TimeSpan.FromSeconds(tokenExpiration - bufferSeconds) : TimeSpan.FromMinutes(30);
        
        _cacheService.Add(cacheKey, tokenData, cacheDuration);

        return tokenData;
    }


    private int GetTokenExpirationTime<T>(T tokenData)
    {
        var tokenType = tokenData?.GetType();
        var expiresInProperty = tokenType?.GetProperty("ExpiresIn");

        return expiresInProperty != null ? (int)(expiresInProperty.GetValue(tokenData) ?? 0) : 0;
    }


    public async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await _httpService.SendAsync(request, cancellationToken);
        var responseJson = JsonConvert.DeserializeObject<T>(response) ?? throw new InvalidOperationException("Failed to deserialize response");

        return responseJson;
    }
}