using DesolaDomain.Entities.Authorization;
using DesolaDomain.Enums;
using DesolaDomain.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Web;
using System.Diagnostics;
using System.Net;
using Desola.Common.Exceptions;

namespace DesolaInfrastructure.Services.Implementations;

public class ApiService : IApiService
{
    private readonly IHttpService _httpService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ApiService> _logger;

    public ApiService(IHttpService httpService, ICacheService cacheService, ILogger<ApiService> logger)
    {
        _httpService = httpService;
        _cacheService = cacheService;
        _logger = logger;
    }
    public async Task<string> FetchAccessTokenAsync(string tokenUrl, string clientId, string clientSecret, string providerName)
    {
        try
        {
            var tokenData = _cacheService.GetItem<TokenAccess>($"{providerName}_{CacheEntry.AccessToken}");

            if (tokenData != null && !tokenData.NeedsRefresh())
            {
                return tokenData.BearerToken ?? string.Empty;
            }

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("client_id",clientId ?? throw new ArgumentNullException(nameof(clientId),"unable to find client id")),
            new KeyValuePair<string, string>("client_secret", clientSecret ?? throw new ArgumentNullException(nameof(clientSecret),"unable to find client secret")),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var headers = new Dictionary<string, string>();

            var jsonContent = await _httpService.PostAsync(tokenUrl, headers, content);
            tokenData = JsonSerializer.Deserialize<TokenAccess>(jsonContent) ?? throw new InvalidOperationException("Failed to deserialize token data");

            if (string.IsNullOrWhiteSpace(tokenData.AccessToken))
            {
                throw new InvalidOperationException("Failed to get access token");
            }

            _cacheService.Add($"{providerName}_{CacheEntry.AccessToken}", tokenData, TimeSpan.FromMilliseconds(tokenData.ExpiresIn - 300)); // 300 seconds buffer

            tokenData.RefreshToken(tokenData.AccessToken, tokenData.ExpiresIn);
            return tokenData.BearerToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }

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


    public async Task<TResponse> CallProviderApiAsync<TRequest, TResponse>(string baseUrl, string flightSearchUrl, HttpMethod method, TRequest requestData, IDictionary<string, string> headers, Func<TRequest, IDictionary<string, string>> parameterMapper, CancellationToken cancellationToken)
    {
        var url = string.IsNullOrEmpty(flightSearchUrl) ? baseUrl : $"{baseUrl.TrimEnd('/')}/{flightSearchUrl.TrimStart('/')}";


        var request = new HttpRequestMessage(method, url);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (requestData != null && (method == HttpMethod.Post || method == HttpMethod.Put))
        {
            var json = JsonSerializer.Serialize(requestData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        if (requestData != null && method == HttpMethod.Get && parameterMapper != null)
        {
            var builder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(builder.Query);

            // Use the provider-specific parameter mapper
            var parameters = parameterMapper(requestData);
            foreach (var param in parameters)
            {
                query[param.Key] = param.Value;
            }

            builder.Query = query.ToString() ?? string.Empty;
            request.RequestUri = builder.Uri;
        }

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(40));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

        var response = await SendAsync<TResponse>(request, linkedCts.Token);

        return response;
    }

    public async Task<T> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await _httpService.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation("API call to {Url} completed in {ElapsedMilliseconds}ms with status {StatusCode}", request.RequestUri?.AbsolutePath, stopwatch.ElapsedMilliseconds, response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("API returned error status code {StatusCode}: {ErrorContent}",
                    response.StatusCode, errorContent);
                
                ThrowAppropriateException(response.StatusCode, errorContent);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (typeof(T) == typeof(string))
            {
                return (T)(object)content;
            }

            var responseJson = JsonConvert.DeserializeObject<T>(content);

            return responseJson;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("API call to {url} was cancelled", request.RequestUri?.AbsoluteUri);
            throw;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("API call to {url} timed out", request.RequestUri?.AbsoluteUri);
            throw new ApiTimeoutException($"Request to  {request.RequestUri?.AbsoluteUri} timed out");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during API call to {BaseUrl}: {Message}",
                request.RequestUri?.AbsoluteUri, ex.Message);

            throw ex.StatusCode switch
            {
                // Translate to domain-specific exceptions based on status code
                HttpStatusCode.Unauthorized => new ApiAuthenticationException("Authentication failed for API call"),
                HttpStatusCode.NotFound => new ApiResourceNotFoundException(
                    $"Resource not found at {request.RequestUri?.AbsoluteUri}"),
                _ => new ApiException($"HTTP error during API call: {ex.Message}", ex)
            };
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response from {BaseUrl}", request.RequestUri?.AbsoluteUri);
            throw new ApiResponseFormatException("Failed to deserialize API response", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during API call to {BaseUrl}: {Message}", request.RequestUri?.AbsoluteUri, ex.Message);
            throw new ApiException($"Unexpected error during API call: {ex.Message}", ex);
        }
    }

    private static void ThrowAppropriateException(HttpStatusCode statusCode, string errorContent)
    {
        try
        {
            var amadeusError = JsonSerializer.Deserialize<AmadeusErrorResponse>(errorContent);
            if (amadeusError?.Errors.Count > 0)
            {
                throw new AmadeusApiException(statusCode, amadeusError);
            }
        }
        catch (System.Text.Json.JsonException) { /* Not an Amadeus error */ }

        // Try to parse as SkyScanner error format
        try
        {
            var skyScannerError = JsonSerializer.Deserialize<SkyScannerErrorResponse>(errorContent);
            if (skyScannerError?.Errors.Count > 0)
            {
                throw new SkyScannerApiException(statusCode, string.Empty, string.Empty);
            }
        }
        catch (System.Text.Json.JsonException) { /* Not a SkyScanner error */ }

        throw new ApiException($"API request failed with status code {statusCode}: {errorContent}");
    }


    private int GetTokenExpirationTime<T>(T tokenData)
    {
        var tokenType = tokenData?.GetType();
        var expiresInProperty = tokenType?.GetProperty("ExpiresIn");

        return expiresInProperty != null ? (int)(expiresInProperty.GetValue(tokenData) ?? 0) : 0;
    }
}