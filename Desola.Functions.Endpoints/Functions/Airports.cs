using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using DesolaDomain.Interfaces;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Newtonsoft.Json;

namespace Desola.Functions.Endpoints.Functions;

public class Airports
{
    private readonly ILogger<Airports> _logger;
    private readonly IAirportRepository _airportRepository;


    public Airports(ILogger<Airports> logger, IAirportRepository airportRepository)
    {
        _logger = logger;
        _airportRepository = airportRepository;
    }

    [Function("Airports")]
    [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "airports")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var airports = await _airportRepository.GetAirportsAsync();

        return new OkObjectResult(airports);
    }
}