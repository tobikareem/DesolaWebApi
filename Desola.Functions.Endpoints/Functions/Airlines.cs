using DesolaDomain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Desola.Functions.Endpoints.Functions
{
    public class Airlines
    {
        private readonly ILogger<Airlines> _logger;
        private readonly  IAirlineRepository _airlineRepository;

        public Airlines(ILogger<Airlines> logger, IAirlineRepository airlineRepository)
        {
            _logger = logger;
            _airlineRepository = airlineRepository;
        }

        [Function("Airlines")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var airlines = await _airlineRepository.GetAmericanAirlinesAsync();
            return new OkObjectResult(airlines.Take(40));
        }
    }
}
