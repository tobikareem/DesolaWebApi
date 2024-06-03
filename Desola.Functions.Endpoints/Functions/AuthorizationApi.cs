using DesolaDomain.Entities.Authorization;
using DesolaDomain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using AuthorizationLevel = Microsoft.Azure.WebJobs.Extensions.Http.AuthorizationLevel;

namespace Desola.Functions.Endpoints.Functions;

public class AuthorizationApi
{
    private readonly IApiService _apiService;

    public AuthorizationApi(IApiService apiService)
    {
        _apiService = apiService;
    }

    [Function("AuthorizationApi")]
    public async Task<IActionResult> Run([HttpTrigger("get", "post", Route = "token")] HttpRequest req)
    {

        var tokenAccess = new TokenAccess
        {
            AccessToken = await _apiService.FetchAccessTokenAsync()
        };

        return new OkObjectResult(tokenAccess.AccessToken);
    }
}

