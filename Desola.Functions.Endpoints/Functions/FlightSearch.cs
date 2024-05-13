using DesolaDomain.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Desola.Functions.Endpoints.Functions;

public class FlightSearch
{
    private readonly ILogger<FlightSearch> _logger;
    private readonly IAirportRepository _airportRepository;
    public FlightSearch(ILogger<FlightSearch> logger, IAirportRepository airportRepository)
    {
        _logger = logger;
        _airportRepository = airportRepository;
    }
    
    [Function("FlightSearch")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "flight/search")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var flights = await _airportRepository.GetAirportsAsync();


        return new OkObjectResult("Welcome to Azure Functions!");
    }
}