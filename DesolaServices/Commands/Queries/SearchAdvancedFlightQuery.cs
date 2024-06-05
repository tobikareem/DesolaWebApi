using DesolaDomain.Aggregates;
using DesolaServices.DataTransferObjects.Requests;
using MediatR;

namespace DesolaServices.Commands.Queries;

public class SearchAdvancedFlightQuery(FlightSearchAdvancedRequest criteria) : IRequest<FlightOffer>
{
    public FlightSearchAdvancedRequest Criteria { get; } = criteria;
}