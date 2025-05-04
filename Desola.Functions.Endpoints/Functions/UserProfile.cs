using AutoMapper;
using DesolaDomain.Entities.User;
using DesolaDomain.Interfaces;
using DesolaServices.Commands.Queries;
using DesolaServices.DataTransferObjects.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Newtonsoft.Json;

namespace Desola.Functions.Endpoints.Functions;

public class UserProfile
{
    private readonly ILogger<UserProfile> _logger;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ITableBase<UserTravelPreference> _userTravelPreferenceTable;
    public UserProfile(ILogger<UserProfile> logger, IMediator mediator, IMapper mapper, ITableBase<UserTravelPreference> userTravelPreferenceTable)
    {
        _logger = logger;
        _mediator = mediator;
        _mapper = mapper;
        _userTravelPreferenceTable = userTravelPreferenceTable;
    }

    [Authorize, RequiredScope("Files.Read")]
    [Function("UserProfile")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "user/store")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var isAuthenticated = await IsUserAuthenticated(req);

        if (!isAuthenticated.authenticationStatus)
        {
            return isAuthenticated.authenticationResponse!;
        }

        req.HttpContext.VerifyUserHasAnyAcceptedScope("Files.Read");

        var name = req.HttpContext.User.Identity is { IsAuthenticated: true } ? req.HttpContext.User.GetDisplayName() : null;

        string responseMessage = string.IsNullOrEmpty(name)
            ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            : $"Hello, {name}. This HTTP triggered function executed successfully.";

        return new OkObjectResult("Welcome to Azure Functions! " + responseMessage);
    }

    [Authorize, RequiredScope("Files.Read")]
    [Function("GetUserTravelPreferences")]
    public async Task<IActionResult> GetUserPreferences(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user/preferences/{userId}")] HttpRequest req, string userId)
    {
        _logger.LogInformation($"Fetching travel preferences for User ID: {userId}");

        var isAuthenticated = await IsUserAuthenticated(req);
        if (!isAuthenticated.authenticationStatus)
        {
            return isAuthenticated.authenticationResponse!;
        }

        req.HttpContext.VerifyUserHasAnyAcceptedScope("Files.Read");

        if (string.IsNullOrEmpty(userId))
        {
            return new UnauthorizedResult();
        }

        var preferences = await _mediator.Send(new GetUserTravelPreferenceQuery(userId));

        if (preferences == null)
        {
            return new NotFoundObjectResult("User travel preferences not found.");
        }

        return new OkObjectResult(preferences);
    }



    [Authorize, RequiredScope("Files.Read")]
    [Function("SaveUserTravelPreferences")]
    public async Task<IActionResult> UserPreferenceSaved(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "user/preferences")] HttpRequest req)
    {
        _logger.LogInformation("Processing user travel preferences update");

        var isAuthenticated = await IsUserAuthenticated(req);

        if (!isAuthenticated.authenticationStatus)
        {
            return isAuthenticated.authenticationResponse!;
        }

        req.HttpContext.VerifyUserHasAnyAcceptedScope("Files.Read");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var preferenceRequest = JsonConvert.DeserializeObject<UserTravelPreferenceRequest>(requestBody);

        if (preferenceRequest == null)
        {
            return new BadRequestObjectResult("Invalid request payload.");
        }

        var preferences = _mapper.Map<UserTravelPreference>(preferenceRequest);

        var existing = await _userTravelPreferenceTable.GetTableEntityAsync(preferences.PartitionKey, preferences.RowKey);
        if (!string.IsNullOrWhiteSpace(existing.UserId))
        {
            await _userTravelPreferenceTable.UpdateTableEntityAsync(preferences);
        }
        else
        {
            await _userTravelPreferenceTable.InsertTableEntityAsync(preferences);
        }


        // await _mediator.Send(new InsertUserTravelPreferenceCommand(preferences));

        return new OkObjectResult("User travel preferences saved successfully.");
    }

    private static async Task<(bool authenticationStatus, IActionResult? authenticationResponse)> IsUserAuthenticated(HttpRequest req) => await req.HttpContext.AuthenticateAzureFunctionAsync();

}