using DesolaDomain.Aggregates;
using DesolaServices.DataTransferObjects.Requests;
using MediatR;

namespace DesolaServices.Commands.Queries;

public class SearchBasicFlightQuery(FlightSearchBasic criteria) : IRequest<FlightOffer>
{
    public FlightSearchBasic Criteria { get; } = criteria;
}