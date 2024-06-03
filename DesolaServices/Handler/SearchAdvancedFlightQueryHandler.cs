using DesolaServices.Interfaces;
using DesolaServices.Queries;
using MediatR;

namespace DesolaServices.Handler;

public class SearchAdvancedFlightQueryHandler : IRequestHandler<SearchAdvancedFlightQuery, amadeus.resources.FlightOffer[]>
{
    private readonly IFlightSearchService _flightSearchService;

    public SearchAdvancedFlightQueryHandler(IFlightSearchService flightSearchService)
    {
        _flightSearchService = flightSearchService;
    }
    public async Task<amadeus.resources.FlightOffer[]> Handle(SearchAdvancedFlightQuery request, CancellationToken cancellationToken)
    {
        return await _flightSearchService.SearchAdvancedFlightsAsync(request.Criteria);
    }
}