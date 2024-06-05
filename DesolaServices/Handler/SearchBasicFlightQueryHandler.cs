using DesolaDomain.Aggregates;
using DesolaServices.Commands.Queries;
using DesolaServices.Interfaces;
using MediatR;

namespace DesolaServices.Handler;

public class SearchBasicFlightQueryHandler : IRequestHandler<SearchBasicFlightQuery, FlightOffer>
{

    private readonly IFlightSearchService _flightSearchService;

    public SearchBasicFlightQueryHandler(IFlightSearchService flightSearchService)
    {
        _flightSearchService = flightSearchService;
    }

    public async Task<FlightOffer> Handle(SearchBasicFlightQuery request, CancellationToken cancellationToken)
    {
        return await _flightSearchService.SearchFlightsAsync(request.Criteria);
    }
}