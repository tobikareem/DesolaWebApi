using DesolaDomain.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Desola.Functions.Endpoints.Functions;

public class FlightSearch(ILogger<FlightSearch> logger, IAirportRepository airportRepository)
{
    [Function("FlightSearch")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "flight/search")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var flights = await airportRepository.GetAirportsAsync();


        return new OkObjectResult("Welcome to Azure Functions!");
    }
}