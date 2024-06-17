using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Commands.Queries;

public class AirportAutoCompleteQuery (string airportQuery) : IRequest<List<AirportAutoCompleteResponse>>
{
    public string AirportSearchQuery { get; } = airportQuery;
}