using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using DesolaDomain.Model;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using DesolaServices.Commands.Queries.Airports;
using DesolaServices.DataTransferObjects.Responses;

namespace Desola.Functions.Endpoints.Functions;

public class Airports
{
    private readonly ILogger<Airports> _logger;
    private readonly IMediator _mediator;

    public Airports(
        ILogger<Airports> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [Function("Airports")]
    [OpenApiOperation("GetAllAirports", tags: new[] { "airports" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<AirportBasicResponse>), Description = "Returns all airports")]
    public async Task<IActionResult> GetAllAirports(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "airports")] HttpRequest req)
    {
        _logger.LogInformation("Processing request to retrieve all airports.");

        var airports = await _mediator.Send(new GetAllAirportsQuery());

        return new OkObjectResult(airports);
    }

    [Function("AirportsAutoComplete")]
    [OpenApiOperation("GetAirportAutoComplete", tags: new[] { "airports" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
    [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Search term for airport autocomplete")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<AirportBasicResponse>), Description = "Autocomplete airport list")]
    public async Task<IActionResult> GetAirportAutoComplete(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "airports/autocomplete")] HttpRequest req)
    {
        _logger.LogInformation("Processing airport autocomplete request.");

        var query = req.Query["name"].ToString();
        if (string.IsNullOrWhiteSpace(query))
        {
            _logger.LogWarning("Missing required 'name' parameter in autocomplete request.");
            return new BadRequestObjectResult(new { error = "The 'name' parameter is required." });
        }

        var airports = await _mediator.Send(new GetAirportAutoCompleteQuery(query));
        return new OkObjectResult(airports);
    }
}
