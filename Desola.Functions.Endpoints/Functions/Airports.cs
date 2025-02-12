using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using DesolaDomain.Interfaces;
using MediatR;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using DesolaServices.Commands.Queries;

namespace Desola.Functions.Endpoints.Functions;

public class Airports
{
    private readonly ILogger<Airports> _logger;
    private readonly IAirportRepository _airportRepository;
    private readonly IMediator _mediator;


    public Airports(ILogger<Airports> logger, IAirportRepository airportRepository, IMediator mediator)
    {
        _logger = logger;
        _airportRepository = airportRepository;
        _mediator = mediator;
    }

    [Function("Airports")]
    [OpenApiOperation(operationId: "Run", tags: new[] { "airports" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
    [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string),
        Description = "The **Name** parameter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string),
        Description = "The OK response")]

    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "airports")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var airports = await _airportRepository.GetAirportsAsync();

        return new OkObjectResult(airports);
    }


    [Function("AirportsAutoComplete")]
    [OpenApiOperation(operationId: "GetAirportAutoComplete", tags: new[] { "airports" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Header)]
    [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string),
        Description = "The **Name** parameter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string),
        Description = "The OK response")]
    public async Task<IActionResult> GetAirportAutoComplete(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "airports/autocomplete")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var query = req.Query["name"].ToString();

        if (string.IsNullOrEmpty(query))
        {
            return new BadRequestObjectResult("Name parameter is required.");
        }

        var airports = await _mediator.Send(new AirportAutoCompleteQuery(query));

        return new OkObjectResult(airports);
    }
}