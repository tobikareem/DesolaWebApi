using DesolaDomain.Aggregates;
using DesolaServices.DataTransferObjects.Requests;
using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Commands.Queries;

public class SearchBasicFlightQuery(FlightSearchBasicRequest criteria) : IRequest<Dictionary<string, FlightItineraryGroupResponse>>
{
    public FlightSearchBasicRequest Criteria { get; } = criteria;
}