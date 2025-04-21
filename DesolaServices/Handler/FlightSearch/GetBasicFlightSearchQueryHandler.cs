using DesolaDomain.Entities.FlightSearch;
using DesolaDomain.Interfaces;
using DesolaServices.Commands.Queries.FlightSearch;
using MediatR;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace DesolaServices.Handler.FlightSearch;

public class GetBasicFlightSearchQueryHandler: IRequestHandler<GetBasicFlightSearchQuery, Tuple<UnifiedFlightSearchResponse, Dictionary<string, string[]>>>
{
    private readonly IFlightProvider _flightProvider;
    private readonly ILogger<GetBasicFlightSearchQueryHandler> _logger;

    public GetBasicFlightSearchQueryHandler(IFlightProvider flightProvider, ILogger<GetBasicFlightSearchQueryHandler> logger)
    {
        _flightProvider = flightProvider;
        _logger = logger;
    }
    public async Task<Tuple<UnifiedFlightSearchResponse, Dictionary<string, string[]>>> Handle(GetBasicFlightSearchQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Getting flight request from {request.SearchParameters.Origin} to {request.SearchParameters.Destination}");

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request.SearchParameters);

        var errors = new Dictionary<string, string[]>();

        if (!Validator.TryValidateObject(request.SearchParameters, validationContext, validationResults, true))
        {
            errors = validationResults
                .GroupBy(vr => string.Join(", ", vr.MemberNames))
                .ToDictionary(
                    g => string.IsNullOrEmpty(g.Key) ? "General" : g.Key,
                    g => g.Select(vr => vr.ErrorMessage).ToArray()
                );
            
        }

        var flightResponse = await _flightProvider.SearchFlightsAsync(request.SearchParameters, cancellationToken);
        return new Tuple<UnifiedFlightSearchResponse, Dictionary<string, string[]>>(flightResponse, errors);
    }
}