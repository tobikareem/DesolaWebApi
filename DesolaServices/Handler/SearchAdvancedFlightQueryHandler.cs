using DesolaDomain.Aggregates;
using DesolaServices.Commands.Queries;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using MediatR;

namespace DesolaServices.Handler;

public class SearchAdvancedFlightQueryHandler : IRequestHandler<SearchAdvancedFlightQuery, Dictionary<string, FlightItineraryGroupResponse>>
{
    private readonly IFlightSearchService _flightSearchService;

    public SearchAdvancedFlightQueryHandler(IFlightSearchService flightSearchService)
    {
        _flightSearchService = flightSearchService;
    }
    public async Task<Dictionary<string, FlightItineraryGroupResponse>> Handle(SearchAdvancedFlightQuery request, CancellationToken cancellationToken)
    {
        return await _flightSearchService.SearchAdvancedFlightsAsync(request.Criteria);
    }
}