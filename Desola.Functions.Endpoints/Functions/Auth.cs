using System.Text.Json;
using DesolaDomain.Model;
using DesolaServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Desola.Functions.Endpoints.Functions;

public class Auth
{
    private readonly ILogger<Auth> _logger;
    private readonly IAuthService _authService;
    public Auth(ILogger<Auth> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    [Function("TokenExchange")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "auth/token")] HttpRequest req)
    {
        _logger.LogInformation("Processing token exchange request...");

        // Read request body
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(requestBody);

        if (data == null || !data.ContainsKey("code"))
        {
            return new BadRequestObjectResult(new { error = "Authorization code is required." });
        }

        var authorizationCode = data["code"];

        try
        {
            // Call Azure AD B2C to exchange code for tokens
            var tokenResponse = await _authService.ExchangeCodeForTokenAsync(authorizationCode);

            return new OkObjectResult(tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for token.");
            return new BadRequestObjectResult(new { error = "Token exchange failed.", details = ex.Message });
        }
    }

    [Function("RefreshToken")]
    public async Task<IActionResult> RefreshToken(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "auth/refresh")] HttpRequest req)
    {
        _logger.LogInformation("Processing refresh token request...");

        try
        {
            // Read and validate request
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<AuthenticationToken>(requestBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request == null || string.IsNullOrEmpty(request.RefreshToken))
            {
                return new BadRequestObjectResult(new { error = "Refresh token is required." });
            }

            // Validate refresh token
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                _logger.LogWarning("Invalid refresh token attempted.");
                return new UnauthorizedObjectResult(new { error = "Invalid refresh token." });
            }

            // Generate new tokens
            var newTokens = await _authService.RefreshTokensAsync(request.RefreshToken);

            if (newTokens == null)
            {
                _logger.LogError("Token refresh failed - null response from auth service");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("Token refresh successful");
            return new OkObjectResult(newTokens);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized refresh token attempt");
            return new UnauthorizedObjectResult(new { error = "Invalid refresh token." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}