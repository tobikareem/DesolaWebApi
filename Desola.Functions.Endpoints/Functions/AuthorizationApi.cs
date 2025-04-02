using DesolaDomain.Entities.Authorization;
using DesolaDomain.Interfaces;
using DesolaDomain.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;
using AuthorizationLevel = Microsoft.Azure.WebJobs.Extensions.Http.AuthorizationLevel;

namespace Desola.Functions.Endpoints.Functions;

public class AuthorizationApi
{
    private readonly IApiService _apiService;
    private readonly AmadeusApi _amadeusConfig;


    public AuthorizationApi(IApiService apiService, IOptions<AppSettings> settingsOptions)
    {
        _apiService = apiService;
        _amadeusConfig = settingsOptions.Value.ExternalApi.Amadeus;
    }

    [Function("AuthorizationApi")]
    public async Task<IActionResult> Run([HttpTrigger("get", "post", Route = "token")] HttpRequest req)
    {

        var tokenAccess = new TokenAccess
        {
            AccessToken = await _apiService.FetchAccessTokenAsync(_amadeusConfig.TokenEndpointUrl, _amadeusConfig.ClientId, _amadeusConfig.ClientSecret, _amadeusConfig.ProviderName)
        };

        return new OkObjectResult(tokenAccess.AccessToken);
    }
}

