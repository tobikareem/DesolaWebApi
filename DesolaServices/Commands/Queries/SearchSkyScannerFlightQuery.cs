using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Commands.Queries;

public class SearchSkyScannerFlightQuery : IRequest<Dictionary<string, FlightItineraryGroupResponse>>
{

    public SearchSkyScannerFlightQuery(SkyScannerFlightRequest flightRequest)
    {
        FlightRequest = flightRequest;
    }
    public SkyScannerFlightRequest FlightRequest { get; }
}