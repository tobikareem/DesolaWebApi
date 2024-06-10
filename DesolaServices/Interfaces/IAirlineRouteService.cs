using amadeus.resources;
using DesolaDomain.Aggregates;
using DesolaServices.DataTransferObjects.Responses;

namespace DesolaServices.Interfaces;

public interface IAirlineRouteService
{
    Task<List<FlightRouteResponse>> GetAirportRoutesAsync(string airlineCode, int max);
}