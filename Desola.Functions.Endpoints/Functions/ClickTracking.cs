using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web.Resource;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Security.Claims;
using DesolaDomain.Entities.User;
using DesolaServices.DataTransferObjects.Requests;
using Newtonsoft.Json;
using AutoMapper;
using DesolaDomain.Interfaces;

namespace Desola.Functions.Endpoints.Functions;

public class ClickTracking
{
    private readonly ILogger<ClickTracking> _logger;
    private readonly ITableBase<UserClickTracking> _userClickTrackingTable;
    private readonly IMapper _mapper;

    public ClickTracking(ILogger<ClickTracking> logger, ITableBase<UserClickTracking> userClickTrackingTable, IMapper mapper)
    {
        _logger = logger;
        _userClickTrackingTable = userClickTrackingTable;
        _mapper = mapper;
    }

    [Function("AddClickTracking")]
    [OpenApiOperation(operationId: "ClickTracking", tags: new[] { "track" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<object>), Description = "Save the user track.")]
    public async Task<IActionResult> AddClickTracking([HttpTrigger(AuthorizationLevel.Function,  "post", Route = "track")] HttpRequest req)
    {
        _logger.LogInformation("Processing User track.");

        var isAuthenticated = await IsUserAuthenticated(req);

        if (!isAuthenticated.authenticationStatus)
        {
            return isAuthenticated.authenticationResponse!;
        }

        req.HttpContext.VerifyUserHasAnyAcceptedScope("Files.Read");

        var userId = req.HttpContext.User.Identity is { IsAuthenticated: true } ? req.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;


        if (string.IsNullOrEmpty(userId))
        {
            return new UnauthorizedResult();
        }

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        var unifiedTrack = JsonConvert.DeserializeObject<ClickTrackingPayload>(requestBody);
        

        if (unifiedTrack == null)
        {
            return new BadRequestObjectResult("Invalid request payload.");
        }

        var preferences = _mapper.Map<UserClickTracking>(unifiedTrack);
        
        var existing = await _userClickTrackingTable.GetTableEntityAsync(preferences.PartitionKey, preferences.RowKey);

        if (existing != null && !string.IsNullOrWhiteSpace(existing.UserId))
        {
            await _userClickTrackingTable.UpdateTableEntityAsync(preferences);
        }
        else
        {
            await _userClickTrackingTable.InsertTableEntityAsync(preferences);
        }

        return new OkObjectResult("Table Tracking information has been failed!");
    }

    [Function("GetClickTracking")]
    [OpenApiOperation(operationId: "ClickTracking", tags: new[] { "track" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json",
        bodyType: typeof(IEnumerable<object>), Description = "Get the user track.")]
    public async Task<IActionResult> GetClickTracking([HttpTrigger(AuthorizationLevel.Function, "get", Route = "track/{userId}")] HttpRequest req, string userId)
    {
        _logger.LogInformation("Processing User track.");
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
        
        var existing = await _userClickTrackingTable.GetTableEntityAsync(userId, "CLICK_TRACKING");

        if (existing == null || string.IsNullOrWhiteSpace(existing.UserId))
            return new NotFoundObjectResult("User travel preferences not found.");

        var preferences = _mapper.Map<ClickTrackingPayload>(existing);
        return new OkObjectResult(preferences);

    }

    private static async Task<(bool authenticationStatus, IActionResult? authenticationResponse)> IsUserAuthenticated(HttpRequest req) => await req.HttpContext.AuthenticateAzureFunctionAsync();

}