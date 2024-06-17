using DesolaServices.DataTransferObjects.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using MediatR;
using Newtonsoft.Json;
using DesolaServices.Commands.Queries;

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
    public async Task<IActionResult> RunBasicSearch([HttpTrigger("get", "post", Route = "flight/search")] HttpRequest req, SearchBasicFlightQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
        var data = JsonConvert.DeserializeObject<FlightSearchBasicRequest>(requestBody);

        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var response = await _mediator.Send(new SearchBasicFlightQuery(new FlightSearchBasicRequest
        {
            Adults = data.Adults,
            Origin = data.Origin,
            Destination = data.Destination,
            DepartureDate = data.DepartureDate,
            ReturnDate = data.ReturnDate,
            MaxResults = data.MaxResults

        }), cancellationToken);
        return new OkObjectResult(response);
    }

    [Function("FlightSearchAdvanced")]
    public async Task<IActionResult> RunAdvancedSearch([HttpTrigger("post", Route = "flight/search/advance")] HttpRequest req, SearchAdvancedFlightQuery query)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonConvert.DeserializeObject<FlightSearchAdvancedRequest>(requestBody);

        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var response = await _mediator.Send(new SearchAdvancedFlightQuery(data));
        return new OkObjectResult(response);
    }

}