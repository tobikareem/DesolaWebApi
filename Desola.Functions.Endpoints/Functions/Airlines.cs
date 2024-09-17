using DesolaDomain.Interfaces;
using DesolaServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Desola.Functions.Endpoints.Functions;

public class Airlines
{
    private readonly ILogger<Airlines> _logger;
    private readonly IAirlineRepository _airlineRepository;
    private readonly IAirlineRouteService _airlineRouteService;

    public Airlines(ILogger<Airlines> logger, IAirlineRepository airlineRepository, IAirlineRouteService airlineRouteService)
    {
        _logger = logger;
        _airlineRepository = airlineRepository;
        _airlineRouteService = airlineRouteService;
    }

    /// <summary>
    /// Get a list of all airlines
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("Airlines")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "airline/all")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var airlines = await _airlineRepository.GetAllAirlinesAsync();
        return new OkObjectResult(airlines.Take(40));
    }


    /// <summary>
    /// Get airline route by passing query parameters
    /// </summary>
    /// <param name="req"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Function("AirlineRoutes")]
    public async Task<IActionResult> GetRoutes([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "airline/route")] HttpRequest req, CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var airlineCode = req.Query["airlineCode"].ToString();
        if (string.IsNullOrEmpty(airlineCode))
        {
            return new BadRequestObjectResult("Airline code is required.");
        }

        if (!int.TryParse(req.Query["max"], out var max))
        {
            max = 60;
        }

        var routes = await _airlineRouteService.GetAirportRoutesAsync(airlineCode, max, cancellationToken);


        if (routes != null)
        {
            return new OkObjectResult(routes);
        }

        _logger.LogWarning("No routes found");
        return new NotFoundResult();

    }
}