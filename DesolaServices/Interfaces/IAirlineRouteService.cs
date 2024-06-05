using amadeus.resources;
using DesolaDomain.Aggregates;

namespace DesolaServices.Interfaces;

public interface IAirlineRouteService
{
    Task<List<RouteLocation>> GetAirportRoutesAsync(string airlineCode, int max);
}