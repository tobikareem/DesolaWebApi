using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.Commands.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using DesolaDomain.Entities.AmadeusFields.Basic;
using DesolaDomain.Interfaces;
using MediatR;
using System.ComponentModel.DataAnnotations;
using Desola.Common;
using Desola.Common.Exceptions;
using DesolaDomain.Entities.FlightSearch;
using DesolaServices.Commands.Queries.FlightSearch;

namespace Desola.Functions.Endpoints.Functions;

public class FlightSearch
{
    private readonly ILogger<FlightSearch> _logger;
    private readonly IMediator _mediator;

    private readonly IFlightProvider _flightProvider;

    public FlightSearch(ILogger<FlightSearch> logger, IMediator mediator, IFlightProvider flightProvider)
    {
        _logger = logger;
        _mediator = mediator;
        _flightProvider = flightProvider;
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

    [Function("AmadeusBasicFlight")]
    [OpenApiOperation("AmadeusFlight", tags: new[] { "Flights" })]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UnifiedFlightSearchResponse), Description = "Returns itineraries from Amadeus source")]
    public async Task<IActionResult> AmadeusFlight(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "flight/search/amadeus")] HttpRequest req,
        CancellationToken cancellationToken)
    {

        try
        {
            // Create parameters object from query string
            var parameters = new FlightSearchParameters
            {
                Origin = req.Query["originLocationCode"],
                Destination = req.Query["destinationLocationCode"],
                CabinClass = req.Query["travelClass"],
                CurrencyCode = string.IsNullOrEmpty(req.Query["currencyCode"]) ? "USD" : req.Query["currencyCode"],

                IncludedAirlineCodes = Utils.ParseCommaSeparatedList(req.Query["includedAirlineCodes"]),
                ExcludedAirlineCodes = Utils.ParseCommaSeparatedList(req.Query["excludedAirlineCodes"])
            };

            // Parse date values
            if (!DateTime.TryParse(req.Query["departureDate"], out var departureDate))
            {
                return new BadRequestObjectResult(new { Error = "Invalid departureDate format. Use YYYY-MM-DD." });
            }
            parameters.DepartureDate = departureDate;

            // Parse optional values with cleaner approach
            if (!string.IsNullOrEmpty(req.Query["returnDate"]))
            {
                if (!DateTime.TryParse(req.Query["returnDate"], out var returnDate))
                {
                    return new BadRequestObjectResult(new { Error = "Invalid returnDate format. Use YYYY-MM-DD." });
                }
                parameters.ReturnDate = returnDate;
            }

            // Parse numeric values with defaults
            parameters.Adults = Utils.ParseIntParameter(req.Query["adults"]) ?? throw new ArgumentException("Adults parameter is required and must be a valid integer");
            parameters.Children = Utils.ParseIntParameter(req.Query["children"]) ?? 0;
            parameters.Infants = Utils.ParseIntParameter(req.Query["infants"]) ?? 0;
            parameters.MaxPrice = Utils.ParseIntParameter(req.Query["maxPrice"]);
            parameters.MaxResults = Utils.ParseIntParameter(req.Query["max"]) ?? 250;
            parameters.NonStop = Utils.ParseBoolParameter(req.Query["nonStop"]) ?? false;

            var (response, errors) = await _mediator.Send(new GetBasicFlightSearchQuery(parameters), cancellationToken);
            

            if (errors.Any())
            {
                return new BadRequestObjectResult(new
                {
                    Message = "Validation failed",
                    Errors = errors
                });
            }
            
            try
            {
                return new OkObjectResult(response);
            }
            catch (AmadeusApiException ex)
            {
                return new ObjectResult(ex.ErrorResponse)
                {
                    StatusCode = (int)ex.StatusCode
                };
            }
        }
        catch (ArgumentException ex)
        {
            return new BadRequestObjectResult(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Amadeus flight search request");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
