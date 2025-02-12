using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Commands.Queries;

public class AirportAutoCompleteQuery : IRequest<List<AirportAutoCompleteResponse>>
{
    public AirportAutoCompleteQuery(string airportQuery)
    {
        AirportSearchQuery = airportQuery;
    }
    public string AirportSearchQuery { get; }
}