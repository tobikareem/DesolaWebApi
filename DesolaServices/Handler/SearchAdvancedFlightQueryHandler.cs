using DesolaDomain.Aggregates;
using DesolaServices.Commands.Queries;
using DesolaServices.Interfaces;
using MediatR;

namespace DesolaServices.Handler;

public class SearchAdvancedFlightQueryHandler : IRequestHandler<SearchAdvancedFlightQuery, FlightOffer>
{
    private readonly IFlightSearchService _flightSearchService;

    public SearchAdvancedFlightQueryHandler(IFlightSearchService flightSearchService)
    {
        _flightSearchService = flightSearchService;
    }
    public async Task<FlightOffer> Handle(SearchAdvancedFlightQuery request, CancellationToken cancellationToken)
    {
        return await _flightSearchService.SearchAdvancedFlightsAsync(request.Criteria);
    }
}