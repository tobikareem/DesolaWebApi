using DesolaServices.Commands.Queries;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using MediatR;

namespace DesolaServices.Handler;

public class SearchBasicFlightQueryHandler : IRequestHandler<SearchBasicFlightQuery, Dictionary<string, FlightItineraryGroupResponse>>
{

    private readonly IFlightSearchService _flightSearchService;

    public SearchBasicFlightQueryHandler(IFlightSearchService flightSearchService)
    {
        _flightSearchService = flightSearchService;
    }

    public async Task<Dictionary<string, FlightItineraryGroupResponse>> Handle(SearchBasicFlightQuery request, CancellationToken cancellationToken)
    {
        return await _flightSearchService.SearchFlightsAsync(request.Criteria, cancellationToken);
    }
}