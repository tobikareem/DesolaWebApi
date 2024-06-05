using DesolaDomain.Interfaces;
using DesolaServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Desola.Functions.Endpoints.Functions
{
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

        [Function("Airlines")]
        public async Task<IActionResult> Run([Microsoft.Azure.Functions.Worker.HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "airline/all")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var airlines = await _airlineRepository.GetAllAirlinesAsync();
            return new OkObjectResult(airlines.Take(40));
        }

        [Function("Routes")]
        public async Task<IActionResult> GetRoutes([Microsoft.Azure.Functions.Worker.HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "airline/route")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string airlineCode = req.Query["airlineCode"];
            if (string.IsNullOrEmpty(airlineCode))
            {
                return new BadRequestObjectResult("Airline code is required.");
            }

            if (!int.TryParse(req.Query["max"], out int max))
            {
                max = 20; // Default value if not provided or invalid
            }

            var routes = await _airlineRouteService.GetAirportRoutesAsync(airlineCode, max);
            return new OkObjectResult(routes.Take(40));
        }
    }
}
