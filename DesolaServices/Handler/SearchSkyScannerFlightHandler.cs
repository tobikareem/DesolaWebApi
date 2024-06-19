using DesolaServices.Commands.Queries;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using DesolaServices.Interfaces;
using MediatR;

namespace DesolaServices.Handler;

public class SearchSkyScannerFlightHandler: IRequestHandler<SearchSkyScannerFlightQuery, Dictionary<string, FlightItineraryGroupResponse>>
{
    private readonly IFlightSearchService _flightSearchService;
    public SearchSkyScannerFlightHandler(IFlightSearchService flightSearchService)
    {
        _flightSearchService = flightSearchService;
    }

    public async Task<Dictionary<string, FlightItineraryGroupResponse>> Handle(SearchSkyScannerFlightQuery request, CancellationToken cancellationToken)
    {
       var response = await _flightSearchService.SearchSkyScannerFlightsAsync(request.FlightRequest, cancellationToken);

       return response;
    }
}