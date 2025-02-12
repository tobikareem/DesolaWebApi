using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Commands.Queries;

public class SearchAdvancedFlightQuery(FlightSearchAdvancedRequest criteria) : IRequest<Dictionary<string, FlightItineraryGroupResponse>>, IRequest<SkyScannerFlightRequest>
{
    public FlightSearchAdvancedRequest Criteria { get; } = criteria;
}