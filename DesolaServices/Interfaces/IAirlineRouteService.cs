using amadeus.resources;

namespace DesolaServices.Interfaces;

public interface IAirlineRouteService
{
    Task<List<Location>> GetAirportRoutesAsync(string airlineCode, int max);
}