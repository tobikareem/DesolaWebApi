using DesolaDomain.Interfaces;
using DesolaServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Net;

namespace Desola.Functions.Endpoints.Functions;

public class Airlines
{
    private readonly ILogger<Airlines> _logger;
    private readonly IAirlineRepository _airlineRepository;
    private readonly IAirlineRouteService _airlineRouteService;

    public Airlines(
        ILogger<Airlines> logger,
        IAirlineRepository airlineRepository,
        IAirlineRouteService airlineRouteService)
    {
        _logger = logger;
        _airlineRepository = airlineRepository;
        _airlineRouteService = airlineRouteService;
    }

    /// <summary>
    /// Get a list of all airlines.
    /// </summary>
    [Function("Airlines")]
    [OpenApiOperation(operationId: "GetAirlines", tags: new[] { "airlines" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(IEnumerable<object>),
        Description = "Returns a list of airlines.")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "airline/all")] HttpRequest req)
    {
        _logger.LogInformation("Getting all airlines.");

        var airlines = await _airlineRepository.GetAllAsync();
        return new OkObjectResult(airlines.Take(40));
    }

    /// <summary>
    /// Get all routes served by the specified airline.
    /// </summary>
    [Function("AirlineRoutes")]
    [OpenApiOperation(operationId: "GetAirlineRoutes", tags: new[] { "airlines" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
    [OpenApiParameter(name: "airlineCode", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The IATA airline code.")]
    [OpenApiParameter(name: "max", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "Maximum number of routes to return (default is 60).")]
    [OpenApiParameter(name: "countryCode", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Optional filter by destination country code.")]
    [OpenApiResponseWithBody(
        statusCode: HttpStatusCode.OK,
        contentType: "application/json",
        bodyType: typeof(IEnumerable<Airlines>),
        Description = "Returns route information served by the airline.")]
    [OpenApiResponseWithoutBody(
        statusCode: HttpStatusCode.NotFound,
        Description = "No routes found for the airline.")]
    public async Task<IActionResult> GetRoutes(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "airline/route")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching airline route information.");

        var airlineCode = req.Query["airlineCode"].ToString();
        if (string.IsNullOrEmpty(airlineCode))
        {
            return new BadRequestObjectResult("Airline code is required.");
        }

        int.TryParse(req.Query["max"], out var max);
        if (max <= 0) max = 60;

        var countryCode = req.Query["countryCode"].ToString();

        var routes = await _airlineRouteService.GetAirportRoutesAsync(airlineCode, max, countryCode, cancellationToken);

        if (routes is null || !routes.Any())
        {
            _logger.LogWarning("No airline routes found.");
            return new NotFoundObjectResult("No routes found for the given airline.");
        }

        return new OkObjectResult(routes);
    }
}
