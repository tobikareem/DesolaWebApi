using DesolaServices.DataTransferObjects.Responses;
using MediatR;

namespace DesolaServices.Commands.Queries.Airports;

public class GetAllAirportsQuery : IRequest<List<AirportBasicResponse>>
{
}