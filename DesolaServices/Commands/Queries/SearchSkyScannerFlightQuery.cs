using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Commands.Queries;

public class SearchSkyScannerFlightQuery(SkyScannerFlightRequest flightRequest) : IRequest<Dictionary<string, FlightItineraryGroupResponse>>
{
    public SkyScannerFlightRequest FlightRequest { get; } = flightRequest;
}