using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.Commands.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using MediatR;

namespace Desola.Functions.Endpoints.Functions;

public class FlightSearch
{
    private readonly ILogger<FlightSearch> _logger;
    private readonly IMediator _mediator;

    public FlightSearch(ILogger<FlightSearch> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [Function("FlightSearch")]
    [OpenApiOperation("SearchBasicFlights", tags: new[] { "Flights" })]
    [OpenApiRequestBody("application/json", typeof(FlightSearchBasicRequest), Required = true, Description = "Basic flight search input")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Dictionary<string, object>), Description = "Returns grouped flight itineraries")]
    public async Task<IActionResult> RunBasicSearch(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "flight/search")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Basic flight search triggered.");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
        var data = JsonConvert.DeserializeObject<FlightSearchBasicRequest>(requestBody);

        if (data is null)
        {
            return new BadRequestObjectResult("Invalid or missing flight search parameters.");
        }

        var result = await _mediator.Send(new SearchBasicFlightQuery(data), cancellationToken);
        return new OkObjectResult(result);
    }

    [Function("FlightSearchAdvanced")]
    [OpenApiOperation("SearchAdvancedFlights", tags: new[] { "Flights" })]
    [OpenApiRequestBody("application/json", typeof(FlightSearchAdvancedRequest), Required = true, Description = "Advanced multi-leg flight search input")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Dictionary<string, object>), Description = "Returns advanced grouped flight itineraries")]
    public async Task<IActionResult> RunAdvancedSearch(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "flight/search/advance")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Advanced flight search triggered.");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
        var data = JsonConvert.DeserializeObject<FlightSearchAdvancedRequest>(requestBody);

        if (data is null)
        {
            return new BadRequestObjectResult("Invalid or missing advanced flight parameters.");
        }

        var result = await _mediator.Send(new SearchAdvancedFlightQuery(data), cancellationToken);
        return new OkObjectResult(result);
    }

    [Function("SkyScannerFlightSearch")]
    [OpenApiOperation("SearchSkyScannerFlights", tags: new[] { "Flights" })]
    [OpenApiRequestBody("application/json", typeof(SkyScannerFlightRequest), Required = true, Description = "SkyScanner search input")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Dictionary<string, object>), Description = "Returns itineraries from SkyScanner source")]
    public async Task<IActionResult> RunSkyScannerSearch(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "flight/search/skyscanner")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("SkyScanner flight search triggered.");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
        var data = JsonConvert.DeserializeObject<SkyScannerFlightRequest>(requestBody);

        if (data is null)
        {
            return new BadRequestObjectResult("Invalid SkyScanner flight request.");
        }

        var result = await _mediator.Send(new SearchSkyScannerFlightQuery(data), cancellationToken);
        return new OkObjectResult(result);
    }
}
