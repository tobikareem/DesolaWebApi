using System.Net;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using DesolaDomain.Entities.User;
using DesolaDomain.Interfaces;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

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
    public async Task<IActionResult> AddClickTracking([HttpTrigger(AuthorizationLevel.Function, "post", Route = "track")] HttpRequest req)
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
        await _userClickTrackingTable.InsertTableEntityAsync(preferences);

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

    [Function("GetUserClickHistory")]
    [OpenApiOperation(operationId: "GetUserClickHistory", tags: new[] { "tracking" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "origin", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Filter by flight origin airport code")]
    [OpenApiParameter(name: "destination", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Filter by flight destination airport code")]
    [OpenApiParameter(name: "startDate", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Filter by click date (start, format: yyyy-MM-dd)")]
    [OpenApiParameter(name: "endDate", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Filter by click date (end, format: yyyy-MM-dd)")]
    [OpenApiParameter(name: "period", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Predefined period (today, yesterday, lastweek, lastmonth)")]
    [OpenApiParameter(name: "pageSize", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Number of results per page (default: 20)")]
    [OpenApiParameter(name: "pageToken", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Continuation token for pagination")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ClickHistoryResponse), Description = "Retrieved click history with pagination")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Unauthorized")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorResponse), Description = "Invalid request parameters")]
    public async Task<IActionResult> GetClickHistory([HttpTrigger(AuthorizationLevel.Function, "get", Route = "flight/trackHistory")] HttpRequest req)
    {
        _logger.LogInformation("Processing click history track.");

        try
        {
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

            string origin = req.Query["origin"];
            string destination = req.Query["destination"];
            string startDate = req.Query["startDate"];
            string endDate = req.Query["endDate"];
            string period = req.Query["period"];
            string continuationToken = req.Query["pageToken"];

            if (string.IsNullOrEmpty(userId))
            {
                return new UnauthorizedResult();
            }

            if (!string.IsNullOrEmpty(period))
            {
                var (start, end) = GetDateRangeFromPeriod(period);
                if (start.HasValue && end.HasValue)
                {
                    startDate = start.Value.ToString("o");
                    endDate = end.Value.ToString("o");
                }
            }

            var pageSize = 20;
            if (int.TryParse(req.Query["pageSize"], out var parsedPageSize) && parsedPageSize is > 0 and <= 100)
            {
                pageSize = parsedPageSize;
            }
            
            var conditions = new List<string> {
            $"PartitionKey eq '{userId}'"

        };

            // Add origin filter if provided
            if (!string.IsNullOrEmpty(origin))
            {
                conditions.Add($"FlightOrigin eq '{origin}'");
            }

            // Add destination filter if provided
            if (!string.IsNullOrEmpty(destination))
            {
                conditions.Add($"FlightDestination eq '{destination}'");
            }

            // Add date range filters if provided
            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var startDateTime))
            {
                conditions.Add($"ClickedAt ge '{startDateTime:o}'");
            }

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var endDateTime))
            {
                // Add one day to make it inclusive of the end date
                endDateTime = endDateTime.AddDays(1).AddSeconds(-1);
                conditions.Add($"ClickedAt le '{endDateTime:o}'");
            }

            // Combine all conditions with AND
            var filterQuery = string.Join(" and ", conditions);
            _logger.LogInformation($"Query filter: {filterQuery}");

            var decodedContinuationToken = string.Empty;
            if (!string.IsNullOrEmpty(continuationToken))
            {
                try
                {
                    decodedContinuationToken = Encoding.UTF8.GetString(Convert.FromBase64String(continuationToken));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error decoding continuation token");
                    return new BadRequestObjectResult(new ErrorResponse
                    {
                        Error = "invalid_token",
                        ErrorDescription = "The provided continuation token is invalid"
                    });
                }
            }

            var (results, nextContinuationToken) = await _userClickTrackingTable.GetTableEntitiesByQueryAsync(filterQuery, pageSize, decodedContinuationToken);

            var response = new ClickHistoryResponse
            {
                Results = results.Select(r => new ClickHistoryItem
                {
                    Id = r.RowKey,
                    ClickedAt = r.ClickedAt,
                    FlightOffer = r.FlightOffer,
                    FlightOrigin = r.FlightOrigin,
                    FlightDestination = r.FlightDestination,
                    Timestamp = r.Timestamp
                }).ToList(),
                PageSize = pageSize,
                TotalResults = results.Count, // Note: This is the page count, not total
                HasMoreResults = !string.IsNullOrEmpty(nextContinuationToken),
                NextPageToken = !string.IsNullOrEmpty(nextContinuationToken)
                    ? Convert.ToBase64String(Encoding.UTF8.GetBytes(nextContinuationToken))
                    : null
            };

            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving click history");
            return new ObjectResult(new ErrorResponse
            {
                Error = "internal_error",
                ErrorDescription = "An error occurred while retrieving click history"
            })
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }

    private static async Task<(bool authenticationStatus, IActionResult? authenticationResponse)> IsUserAuthenticated(HttpRequest req) => await req.HttpContext.AuthenticateAzureFunctionAsync();

    private static (DateTime? start, DateTime? end) GetDateRangeFromPeriod(string period)
    {
        var now = DateTime.UtcNow;

        return period.ToLowerInvariant() switch
        {
            "today" => (now.Date, now.Date.AddDays(1).AddSeconds(-1)),
            "yesterday" => (now.Date.AddDays(-1), now.Date.AddSeconds(-1)),
            "lastweek" => (now.Date.AddDays(-7), now.Date.AddSeconds(-1)),
            "lastmonth" => (now.Date.AddMonths(-1), now.Date.AddSeconds(-1)),
            _ => (null, null)
        };
    }

}