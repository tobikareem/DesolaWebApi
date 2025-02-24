using DesolaDomain.Enums;
using DesolaDomain.Interfaces;
using DesolaDomain.Model;
using DesolaServices.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DesolaServices.Services;

public class AuthService : IAuthService
{
    private readonly IApiService _apiService;
    private readonly IConfiguration _configuration;

    public AuthService(IApiService apiService, IConfiguration configuration)
    {
        _apiService = apiService;
        _configuration = configuration;
    }


    public async Task<AuthenticationToken> ExchangeCodeForTokenAsync(string code)
    {
        var tokenEndpoint = $"{_configuration["AzureB2C_Authority"]}/{_configuration["AzureB2C_Policy"]}/oauth2/v2.0/token";
        var parameters = new Dictionary<string, string>
        {
            { "client_id", _configuration["AzureB2C_ClientId"] },
            { "code_verifier", _configuration["AzureB2C_Code_Verifier"] },
            { "scope", $"openid offline_access {_configuration["AzureB2C_Application_Scope"]}" },
            { "grant_type", "authorization_code" },
            { "code", code},
            { "redirect_uri", _configuration["AzureB2C_RedirectUri"] }
        };

        var response = await _apiService.GetAccessTokenAsync<AuthenticationToken>(CacheEntry.MicrosoftB2C, tokenEndpoint, parameters);

        return response;

    }

    public async Task<AuthenticationToken> RefreshTokensAsync(string refreshToken)
    {
        var tokenUrl = $"{_configuration["AzureB2C:Authority"]}/oauth2/v2.0/token?p={_configuration["AzureB2C:Policy"]}";

        var parameters = new Dictionary<string, string>
        {
            { "client_id", _configuration["AzureB2C_ClientId"] },
            { "code_verifier", _configuration["AzureB2C_Code_Verifier"] },
            { "scope", "openid offline_access" },
            {"grant_type", "refresh_token"},
            { "refresh_token", refreshToken},
        };

        var response = await _apiService.GetAccessTokenAsync<AuthenticationToken>(CacheEntry.MicrosoftB2C, tokenUrl, parameters);

        return response;
    }
}