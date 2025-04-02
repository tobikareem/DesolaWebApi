using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Commands.Queries.Airports;

public class GetAirportAutoCompleteQuery : IRequest<List<AirportBasicResponse>>
{
    public GetAirportAutoCompleteQuery(string airportQuery)
    {
        AirportSearchQuery = airportQuery;
    }
    public string AirportSearchQuery { get; }
}